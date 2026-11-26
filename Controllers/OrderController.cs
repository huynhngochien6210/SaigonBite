using Microsoft.AspNetCore.Mvc;
using SaigonBite.Models;
using SaigonBite.Repositories;
using SaigonBite.Services;
using SaigonBite.Services.Payment;
using SaigonBite.ViewModels;
using System.Text.Json;

namespace SaigonBite.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly IOrderRepository _orderRepository;

        public OrderController(
            OrderService orderService,
            IOrderRepository orderRepository)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            return View();
        }


        // =========================================================
        // ĐẶT HÀNG - BƯỚC 1
        // =========================================================

        [HttpGet]
        public IActionResult IndexOrder()
        {
            const string CartKey = "Cart";

            var json = HttpContext.Session.GetString(CartKey);

            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction(
                    "Index",
                    "Cart");
            }

            var cart = JsonSerializer.Deserialize<List<CartItem>>(json)
                       ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction(
                    "Index",
                    "Cart");
            }

            var request = new PlaceOrderRequest
            {
                CustomerId = 1,
                HubId = 1,

                Items = cart.Select(item => new OrderItemRequest
                {
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),

                QuietKitchenDiscount = 0,
                DeliveryFee = 15000,
                RushHourSurcharge = 0,
                HeavyRainSurcharge = 0,

                PaymentMethod = "COD"
            };

            return View("IndexOrder", request);
        }


        // =========================================================
        // ĐẶT HÀNG → XÁC NHẬN
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> IndexOrder(
            PlaceOrderRequest request)
        {
            // Kiểm tra địa chỉ
            if (string.IsNullOrWhiteSpace(
                request.DeliveryAddress))
            {
                ModelState.AddModelError(
                    "DeliveryAddress",
                    "Vui lòng nhập địa chỉ giao hàng.");
            }

            // Kiểm tra khoảng cách
            if (request.DistanceKm <= 0)
            {
                ModelState.AddModelError(
                    "DistanceKm",
                    "Vui lòng nhập khoảng cách giao hàng.");
            }
            else if (request.DistanceKm > 8)
            {
                ModelState.AddModelError(
                    "DistanceKm",
                    "Khoảng cách giao hàng không được vượt quá 8 km.");
            }

            // Kiểm tra giỏ hàng
            if (request.Items == null ||
                !request.Items.Any())
            {
                return RedirectToAction(
                    "Index",
                    "Cart");
            }

            if (!ModelState.IsValid)
            {
                return View("IndexOrder", request);
            }

            // Tính voucher để hiển thị ở Confirm
            if (request.VoucherId.HasValue)
            {
                var voucher =
                    await _orderRepository
                        .GetVoucherByIdAsync(
                            request.VoucherId.Value);

                if (voucher != null)
                {
                    decimal subtotal = request.Items.Sum(
                        item =>
                            item.UnitPrice *
                            item.Quantity);

                    request.VoucherDiscount =
                        voucher.CalculateDiscount(
                            subtotal);
                }
            }
            else
            {
                request.VoucherDiscount = 0;
            }

            return View("Confirm", request);
        }


        // =========================================================
        // XÁC NHẬN - BƯỚC 2
        // =========================================================

        [HttpGet]
        public IActionResult Confirm()
        {
            // Không cho truy cập Confirm trực tiếp.
            // Người dùng phải đi qua trang Đặt hàng.
            return RedirectToAction(
                nameof(IndexOrder));
        }


        [HttpPost]
        public async Task<IActionResult> Confirm(
            PlaceOrderRequest request)
        {
            try
            {
                if (request.Items == null ||
                    !request.Items.Any())
                {
                    return RedirectToAction(
                        "Index",
                        "Cart");
                }

                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    HubId = request.HubId,
                    DeliveryAddress = request.DeliveryAddress,
                    VoucherId = request.VoucherId,

                    OrderItems = request.Items
                        .Select(item => new OrderItem
                        {
                            ItemName = item.ItemName,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        })
                        .ToList()
                };

                IPaymentStrategy paymentStrategy =
                    request.PaymentMethod switch
                    {
                        "COD" => new CODPayment(),
                        "MoMo" => new MoMoPayment(),
                        "VNPay" => new VNPayPayment(),
                        "ApplePay" => new ApplePayPayment(),
                        "PayPal" => new PayPalPayment(),

                        _ => throw new InvalidOperationException(
                            "Invalid payment method.")
                    };

                await _orderService.CreateOrderAsync(
                    order,
                    request.DistanceKm,
                    request.QuietKitchenDiscount,
                    request.DeliveryFee,
                    request.RushHourSurcharge,
                    request.HeavyRainSurcharge,
                    paymentStrategy);

                // Đặt hàng thành công → xóa giỏ hàng
                HttpContext.Session.Remove("Cart");

                return RedirectToAction(
                    nameof(Complete),
                    new
                    {
                        id = order.OrderId
                    });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View("Confirm", request);
            }
        }


        // =========================================================
        // HOÀN TẤT - BƯỚC 3
        // =========================================================

        public IActionResult Complete(int id)
        {
            ViewBag.OrderId = id;

            return View();
        }


        // =========================================================
        // XEM DANH SÁCH ĐƠN
        // =========================================================

        public async Task<IActionResult> Status()
        {
            // Tạm thời dùng CustomerId = 1
            var orders =
                await _orderService
                    .GetCustomerOrdersAsync(1);

            return View(orders);
        }


        // =========================================================
        // CHI TIẾT ĐƠN
        // =========================================================

        public async Task<IActionResult> Details(int id)
        {
            var order =
                await _orderService
                    .GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        // =========================================================
        // CHỈNH SỬA ĐƠN
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order =
                await _orderService
                    .GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Placed)
            {
                return Content(
                    "Order can only be edited while it is Placed.");
            }

            return View(order);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(
            int id,
            List<OrderItem> items)
        {
            try
            {
                await _orderService.EditOrderAsync(
                    id,
                    items,
                    0,
                    15000,
                    0,
                    0);

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = id
                    });
            }
            catch (Exception ex)
            {
                return Content(
                    ex.ToString());
            }
        }


        // =========================================================
        // UPDATE STATUS
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(
            int id,
            OrderStatus status)
        {
            try
            {
                await _orderService
                    .UpdateOrderStatusAsync(
                        id,
                        status);

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = id
                    });
            }
            catch (Exception ex)
            {
                return Content(
                    ex.Message);
            }
        }


        // =========================================================
        // CANCEL
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Cancel(
            int id)
        {
            try
            {
                await _orderService
                    .CancelOrderAsync(id);

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = id
                    });
            }
            catch (Exception ex)
            {
                return Content(
                    ex.Message);
            }
        }
    }
}