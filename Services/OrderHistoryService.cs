using SaigonBite.ViewModels;

namespace SaigonBite.Services
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly ICustomerAccountService
            _customerAccountService;

        private readonly OrderService _orderService;

        public OrderHistoryService(
            ICustomerAccountService customerAccountService,
            OrderService orderService)
        {
            _customerAccountService = customerAccountService;
            _orderService = orderService;
        }

        public async Task<OrderHistoryViewModel>
            GetOrderHistoryAsync(int userId)
        {
            var model = new OrderHistoryViewModel();

            // 1. Find CustomerId from authenticated UserId
            var customerId = await _customerAccountService
                .GetCustomerIdAsync(userId);

            // 2. Customer has no linked order profile
            if (!customerId.HasValue)
            {
                return model;
            }

            // 3. Retrieve customer orders
            var orders = await _orderService
                .GetCustomerOrdersAsync(customerId.Value);

            // 4. Convert database entities to ViewModels
            model.Orders = orders
                .OrderByDescending(o => o.OrderTime)
                .Select(o => new OrderHistoryItemViewModel
                {
                    OrderId = o.OrderId,

                    OrderDate = o.OrderTime,

                    Total = o.FinalTotal,

                    Status = o.Status,

                    DeliveryAddress = o.DeliveryAddress,

                    Items = o.OrderItems
                        .Select(item =>
                            new OrderHistoryProductViewModel
                            {
                                ItemName = item.ItemName,

                                Quantity = item.Quantity,

                                UnitPrice = item.UnitPrice
                            })
                        .ToList()
                })
                .ToList();

            return model;
        }
    }
}