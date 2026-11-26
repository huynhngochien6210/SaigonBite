
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using SaigonBite.Models;
using SaigonBite.Repositories;
using SaigonBite.Services;
using SaigonBite.Services.Payment;
using SaigonBite.ViewModels;

namespace SaigonBite.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerAccountService _customerAccountService;

        public OrderController(
            OrderService orderService,
            IOrderRepository orderRepository,
            ICustomerAccountService customerAccountService)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _customerAccountService = customerAccountService;
        }

        // ==========================================
        // 1. GET CURRENT USER ID
        // ==========================================

        private int? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(userId, out int id) && id > 0)
            {
                return id;
            }

            return null;
        }

        // ==========================================
        // 2. CHECK ORDER OWNERSHIP
        // ==========================================

        private async Task<bool> CanAccessOrderAsync(int orderId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return false;
            }

            return await _customerAccountService
                .OwnsOrderAsync(userId.Value, orderId);
        }

        // ==========================================
        // 3. INDEX
        // ==========================================

        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // 4. CONFIRM ORDER - GET
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Confirm()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Forbid();
            }

            const string CartKey = "Cart";

            var json = HttpContext.Session.GetString(CartKey);

            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Index", "Cart");
            }

            var cart = JsonSerializer.Deserialize<List<CartItem>>(json)
                       ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var customerId = await _customerAccountService
                .GetCustomerIdAsync(userId.Value);

            var request = new PlaceOrderRequest
            {
                CustomerId = customerId ?? 0,

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

        // ==========================================
        // 5. CONFIRM ORDER - POST
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            PlaceOrderRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid order information.");
            }

            // BR13: Payment methods by account type

            var accountType = User.FindFirstValue("AccountType");

            bool validPayment = accountType switch
            {
                "LocalCustomer" =>
                    request.PaymentMethod is "MoMo" or "VNPay" or "COD",

                "ForeignTourist" =>
                    request.PaymentMethod is "ApplePay" or "PayPal" or "COD",

                _ => false
            };

            if (!validPayment)
            {
                return BadRequest(
                    "Payment method is not allowed for this account.");
            }

            // Read cart from server session

            var json = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Index", "Cart");
            }

            var cart = JsonSerializer.Deserialize<List<CartItem>>(json)
                       ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                // Get customer from authenticated account

                var customer = await _customerAccountService
                    .GetOrCreateCustomerAsync(userId.Value);

                var order = new Order
                {
                    CustomerId = customer.CustomerId,

                    HubId = request.HubId,

                    DeliveryAddress = request.DeliveryAddress,

                    VoucherId = request.VoucherId,

                    OrderItems = cart.Select(item => new OrderItem
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==========================================
        // 6. ORDER COMPLETE
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Complete(int id)
        {
            if (!await CanAccessOrderAsync(id))
            {
                return NotFound();
            }

            ViewBag.OrderId = id;

            return View();
        }

        // ==========================================
        // 7. ORDER STATUS
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Status()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Forbid();
            }

            var customerId = await _customerAccountService
                .GetCustomerIdAsync(userId.Value);

            if (customerId == null)
            {
                return View(new List<Order>());
            }

            var orders = await _orderService
                .GetCustomerOrdersAsync(customerId.Value);

            return View(orders);
        }

        // ==========================================
        // 8. ORDER DETAILS
        // ==========================================

        [Authorize(Roles = "Customer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!User.IsInRole("Admin") &&
                !await CanAccessOrderAsync(id))
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==========================================
        // 9. INDEX ORDER
        // ==========================================

        public IActionResult IndexOrder()
        {
            return View("IndexOrder");
        }

        // ==========================================
        // 10. EDIT ORDER - GET
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!await CanAccessOrderAsync(id))
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Placed)
            {
                return BadRequest(
                    "Order can only be edited while it is Placed.");
            }

            return View(order);
        }

        // ==========================================
        // 11. EDIT ORDER - POST
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            List<OrderItem> items)
        {
            if (!await CanAccessOrderAsync(id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid order information.");
            }

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
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==========================================
        // 12. UPDATE ORDER STATUS
        // ==========================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            OrderStatus status)
        {
            if (!ModelState.IsValid ||
                !Enum.IsDefined(typeof(OrderStatus), status))
            {
                return BadRequest("Invalid order status.");
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(
                    id,
                    status);

                return RedirectToAction(
                    nameof(Details),
                    new { id = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==========================================
        // 13. CANCEL ORDER
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!await CanAccessOrderAsync(id))
            {
                return NotFound();
            }

            try
            {
                await _orderService.CancelOrderAsync(id);

                return RedirectToAction(
                    nameof(Details),
                    new { id = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
