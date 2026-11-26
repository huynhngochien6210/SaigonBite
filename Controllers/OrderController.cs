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

        [HttpGet]
        public async Task<IActionResult> Confirm()
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
                DistanceKm = 3,
                DeliveryAddress =
                    "19 Nguyễn Trãi, Quận 1, TP.HCM",

                VoucherId = 1,

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

            // Tính voucher để hiển thị trên Confirm
            var voucher = await _orderRepository
                .GetVoucherByIdAsync(1);

            decimal subtotal = request.Items.Sum(
                item => item.UnitPrice * item.Quantity);

            if (voucher != null)
            {
                request.VoucherDiscount =
                    voucher.CalculateDiscount(subtotal);
            }

            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(PlaceOrderRequest request)
        {
            try
            {
                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    HubId = request.HubId,
                    DeliveryAddress = request.DeliveryAddress,
                    VoucherId = request.VoucherId,

                    OrderItems = request.Items.Select(item => new OrderItem
                    {
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                IPaymentStrategy paymentStrategy =
                    request.PaymentMethod switch
                    {
                        "MoMo" => new MoMoPayment(),
                        "VNPay" => new VNPayPayment(),
                        "COD" => new CODPayment(),
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

                HttpContext.Session.Remove("Cart");

                return RedirectToAction(
                    nameof(Complete),
                    new { id = order.OrderId });
            }
            //catch (Exception ex)
            //{
            //    ModelState.AddModelError("", ex.Message);

            //    return View(request);
            //}

            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public IActionResult Complete(int id)
        {
            ViewBag.OrderId = id;

            return View();
        }

        public async Task<IActionResult> Status()
        {
            // Tạm thời dùng CustomerId = 1
            var orders = await _orderService.GetCustomerOrdersAsync(1);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult IndexOrder()
        {
            return View("IndexOrder");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderAsync(id);

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
                    new { id = id });
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(
            int id,
            OrderStatus status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(
                    id,
                    status);

                return RedirectToAction(
                    nameof(Details),
                    new { id = id });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);

                return RedirectToAction(
                    nameof(Details),
                    new { id = id });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}