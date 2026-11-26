using SaigonBite.Models;
using SaigonBite.Repositories;
using SaigonBite.Services.Payment;

namespace SaigonBite.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly PricingService _pricingService;

        public OrderService(
            IOrderRepository orderRepository,
            PricingService pricingService)
        {
            _orderRepository = orderRepository;
            _pricingService = pricingService;
        }

        public async Task<Order?> GetOrderAsync(int orderId)
        {
            return await _orderRepository.GetByIdAsync(orderId);
        }

        public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
        {
            return await _orderRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<Order> CreateOrderAsync(
            Order order,
            decimal distanceKm,
            decimal quietKitchenDiscount,
            decimal deliveryFee,
            decimal rushHourSurcharge,
            decimal heavyRainSurcharge,
            IPaymentStrategy paymentStrategy)
        {
            // Kiểm tra khoảng cách giao hàng
            if (distanceKm > 8)
            {
                throw new InvalidOperationException(
                    "Delivery distance must not exceed 8 km.");
            }

            // Kiểm tra Hub
            var hub = await _orderRepository.GetHubByIdAsync(order.HubId);

            if (hub == null)
            {
                throw new InvalidOperationException(
                    "Hub does not exist.");
            }

            if (!hub.IsActive)
            {
                throw new InvalidOperationException(
                    "Selected Hub is not active.");
            }

            decimal loadPercentage =
                (decimal)hub.CurrentLoad / hub.Capacity * 100;

            if (loadPercentage >= 90)
            {
                throw new InvalidOperationException(
                    "Hub load has reached 90%.");
            }

            // 1. Tính subtotal
            // Validate items
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                throw new InvalidOperationException(
                    "Order must contain at least one item.");
            }

            foreach (var item in order.OrderItems)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                {
                    throw new InvalidOperationException(
                        "Item name cannot be empty.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid quantity for item: {item.ItemName}");
                }

                if (item.UnitPrice <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid price for item: {item.ItemName}");
                }
            }



            decimal subtotal = order.OrderItems.Sum(
                item => item.UnitPrice * item.Quantity);

            // 2. Kiểm tra subtotal tối thiểu
            if (subtotal < 50000)
            {
                throw new InvalidOperationException(
                    "Minimum order subtotal is 50,000 VND.");
            }

            // 3. Tính voucher discount
            decimal voucherDiscount = 0;

            if (order.VoucherId.HasValue)
            {
                var voucher = await _orderRepository
                    .GetVoucherByIdAsync(order.VoucherId.Value);

                if (voucher != null)
                {
                    voucherDiscount =
                        voucher.CalculateDiscount(subtotal);
                }
            }

            // 4. Pricing
            PricingResult pricing = _pricingService.Calculate(
                subtotal,
                quietKitchenDiscount,
                voucherDiscount,
                deliveryFee,
                rushHourSurcharge,
                heavyRainSurcharge);

            // 5. Payment
            var paymentContext = new PaymentContext();

            paymentContext.SetStrategy(paymentStrategy);

            bool paymentSuccess =
                await paymentContext.PayAsync(pricing.FinalTotal);

            if (!paymentSuccess)
            {
                throw new InvalidOperationException(
                    "Payment failed.");
            }

            // 6. Lưu Final Total
            order.FinalTotal = pricing.FinalTotal;

            // 7. Trạng thái ban đầu
            order.Status = OrderStatus.Placed;

            // 8. Thời gian đặt hàng
            order.OrderTime = DateTime.Now;

            // 9. Lưu Order

            // Increase hub load after successfully creating the order
            hub.CurrentLoad++;

            order.Hub = hub;
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order;
        }

        public async Task<Order> EditOrderAsync(
            int orderId,
            List<OrderItem> updatedItems,
            decimal quietKitchenDiscount,
            decimal deliveryFee,
            decimal rushHourSurcharge,
            decimal heavyRainSurcharge)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                throw new InvalidOperationException(
                    "Order does not exist.");
            }

            // Chỉ được sửa khi đơn đang ở trạng thái Placed
            if (order.Status != OrderStatus.Placed)
            {
                throw new InvalidOperationException(
                    "Order can only be edited while it is Placed.");
            }

            // Kiểm tra danh sách món
            if (updatedItems == null || !updatedItems.Any())
            {
                throw new InvalidOperationException(
                    "Order must contain at least one item.");
            }

            // Validate từng món
            foreach (var item in updatedItems)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                {
                    throw new InvalidOperationException(
                        "Item name cannot be empty.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid quantity for item: {item.ItemName}");
                }

                if (item.UnitPrice <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid price for item: {item.ItemName}");
                }
            }

            // Tính subtotal mới
            decimal subtotal = updatedItems.Sum(
                item => item.UnitPrice * item.Quantity);

            // BR: Minimum subtotal = 50,000
            if (subtotal < 50000)
            {
                throw new InvalidOperationException(
                    "Minimum order subtotal is 50,000 VND.");
            }

            // Tính voucher lại theo subtotal mới
            decimal voucherDiscount = 0;

            if (order.VoucherId.HasValue)
            {
                var voucher = await _orderRepository
                    .GetVoucherByIdAsync(order.VoucherId.Value);

                if (voucher != null)
                {
                    voucherDiscount =
                        voucher.CalculateDiscount(subtotal);
                }
            }

            // Tính giá mới
            var pricing = _pricingService.Calculate(
                subtotal,
                quietKitchenDiscount,
                voucherDiscount,
                deliveryFee,
                rushHourSurcharge,
                heavyRainSurcharge);

            // Xóa các OrderItem cũ
            order.OrderItems.Clear();

            // Thêm các OrderItem mới
            foreach (var item in updatedItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    OrderId = order.OrderId
                });
            }

            // Cập nhật tổng tiền
            order.FinalTotal = pricing.FinalTotal;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order;
        }

        public async Task<Order> UpdateOrderStatusAsync(
    int orderId,
    OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                throw new InvalidOperationException(
                    "Order does not exist.");
            }

            bool isValidTransition =
                order.Status switch
                {
                    OrderStatus.Placed =>
                        newStatus == OrderStatus.Confirmed ||
                        newStatus == OrderStatus.Cancelled,

                    OrderStatus.Confirmed =>
                        newStatus == OrderStatus.Preparing ||
                        newStatus == OrderStatus.Cancelled,

                    OrderStatus.Preparing =>
                        newStatus == OrderStatus.Ready,

                    OrderStatus.Ready =>
                        newStatus == OrderStatus.OutForDelivery,

                    OrderStatus.OutForDelivery =>
                        newStatus == OrderStatus.Delivered ||
                        newStatus == OrderStatus.Failed,

                    OrderStatus.Delivered =>
                        false,

                    OrderStatus.Failed =>
                        false,

                    OrderStatus.Cancelled =>
                        false,

                    _ => false
                };

            if (!isValidTransition)
            {
                throw new InvalidOperationException(
                    $"Cannot change order status from {order.Status} to {newStatus}.");
            }

            order.Status = newStatus;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order;
        }

        public async Task<Order> CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                throw new InvalidOperationException(
                    "Order does not exist.");
            }

            // Chỉ được hủy khi đơn đang Placed hoặc Confirmed
            if (order.Status != OrderStatus.Placed &&
                order.Status != OrderStatus.Confirmed)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled while it is Placed or Confirmed.");
            }

            order.Status = OrderStatus.Cancelled;

            // Decrease hub load when order is cancelled
            if (order.Hub != null && order.Hub.CurrentLoad > 0)
            {
                order.Hub.CurrentLoad--;
            }

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order;
        }


    }
}