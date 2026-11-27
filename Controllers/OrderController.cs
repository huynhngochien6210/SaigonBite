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

        private const string CheckoutKey = "OrderCheckout";

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
        // 3. READ CART FROM SESSION
        // ==========================================

        private List<OrderItemRequest> ReadCart()
        {
            var json = HttpContext.Session.GetString("Cart");

            var cart = string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json)
                    ?? new List<CartItem>();

            return cart.Select(item => new OrderItemRequest
            {
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();
        }

        // ==========================================
        // 4. PAYMENT VALIDATION - BR13
        // ==========================================

        private bool IsPaymentAllowed(string method)
        {
            var accountType = User.FindFirstValue("AccountType");

            return accountType switch
            {
                "LocalCustomer" =>
                    method is "MoMo" or "VNPay" or "COD",

                "ForeignTourist" =>
                    method is "ApplePay" or "PayPal" or "COD",

                _ => false
            };
        }

        // ==========================================
        // 5. CALCULATE VOUCHER
        // ==========================================

                private async Task CalculateVoucherAsync(
            PlaceOrderRequest request)
        {
            request.VoucherDiscount = 0;

            // No voucher selected
            if (!request.VoucherId.HasValue ||
                request.VoucherId.Value == 0)
            {
                request.VoucherId = null;
                return;
            }

            // Find voucher in database
            var voucher = await _orderRepository
                .GetVoucherByIdAsync(request.VoucherId.Value);

            // Reject invalid voucher
            if (voucher == null)
            {
                ModelState.AddModelError(
                    "VoucherId",
                    "Voucher không tồn tại.");

                return;
            }

            // Calculate order subtotal
            decimal subtotal = request.Items.Sum(
                item => item.UnitPrice * item.Quantity);

            // Calculate voucher discount
            request.VoucherDiscount =
                voucher.CalculateDiscount(subtotal);
        }

        // ==========================================
        // 7. INDEX ORDER - GET
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> IndexOrder()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Forbid();
            }

            var items = ReadCart();

            if (items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            HttpContext.Session.Remove(CheckoutKey);

            var customerId = await _customerAccountService
                .GetCustomerIdAsync(userId.Value);

            var request = new PlaceOrderRequest
            {
                CustomerId = customerId ?? 0,

                HubId = 1,

                Items = items,

                QuietKitchenDiscount = 0,

                DeliveryFee = 15000,

                RushHourSurcharge = 0,

                HeavyRainSurcharge = 0,

                PaymentMethod = "COD"
            };

            return View("IndexOrder", request);
        }

        // ==========================================
        // 8. INDEX ORDER - POST
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexOrder(
            PlaceOrderRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Forbid();
            }

            HttpContext.Session.Remove(CheckoutKey);

            // Read cart from server session

            request.Items = ReadCart();

            if (request.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Get CustomerId from authenticated user

            request.CustomerId = await _customerAccountService
                .GetCustomerIdAsync(userId.Value) ?? 0;

            // Remove validation for server-controlled fields

            foreach (var key in ModelState.Keys.Where(k =>
                k == "CustomerId" ||
                k == "Items" ||
                k.StartsWith("Items[") ||
                k.StartsWith("Items.")).ToList())
            {
                ModelState.Remove(key);
            }

            // Validate delivery address

            if (string.IsNullOrWhiteSpace(request.DeliveryAddress) ||
                request.DeliveryAddress.Length > 255)
            {
                ModelState.AddModelError(
                    "DeliveryAddress",
                    "Vui lòng nhập địa chỉ giao hàng hợp lệ.");
            }

            // Validate delivery distance

            if (request.DistanceKm <= 0 ||
                request.DistanceKm > 8)
            {
                ModelState.AddModelError(
                    "DistanceKm",
                    "Khoảng cách giao hàng phải lớn hơn 0 và không quá 8 km.");
            }

            // Validate payment method - BR13

            if (!IsPaymentAllowed(request.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Phương thức thanh toán không phù hợp với tài khoản.");
            }

            if (!ModelState.IsValid)
            {
                return View("IndexOrder", request);
            }

            // Calculate voucher discount

            await CalculateVoucherAsync(request);

            if (!ModelState.IsValid)
            {
                return View("IndexOrder", request);
            }

            // Store checkout information in server session

            HttpContext.Session.SetString(
                CheckoutKey,
                JsonSerializer.Serialize(request));

            HttpContext.Session.SetInt32(
                CheckoutKey + "UserId",
                userId.Value);

            return View("Confirm", request);
        }

        // ==========================================
        // 9. CONFIRM ORDER - GET
        // ==========================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Confirm()
        {
            return RedirectToAction(nameof(IndexOrder));
        }

        // ==========================================
        // 10. CONFIRM ORDER - POST
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

            // Read checkout information from session

            var json = HttpContext.Session.GetString(
                CheckoutKey);

            var checkoutUserId = HttpContext.Session.GetInt32(
                CheckoutKey + "UserId");

            if (string.IsNullOrEmpty(json) ||
                checkoutUserId != userId.Value)
            {
                return RedirectToAction(nameof(IndexOrder));
            }

            var saved = JsonSerializer
                .Deserialize<PlaceOrderRequest>(json);

            if (saved == null)
            {
                return RedirectToAction(nameof(IndexOrder));
            }

            // Use saved checkout instead of client hidden inputs

            request = saved;

            ModelState.Clear();

            // Check current cart

            var items = ReadCart();

            if (items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Return to checkout if cart has changed

            if (JsonSerializer.Serialize(items) !=
                JsonSerializer.Serialize(request.Items))
            {
                return RedirectToAction(nameof(IndexOrder));
            }

            // Validate payment method again

            if (!IsPaymentAllowed(request.PaymentMethod))
            {
                return Forbid();
            }

            await CalculateVoucherAsync(request);

            if (!ModelState.IsValid)
            {
                return View("Confirm", request);
            }

            try
            {
                // Get or create customer linked to UserAccount

                var customer = await _customerAccountService
                    .GetOrCreateCustomerAsync(userId.Value);

                // Create order

                var order = new Order
                {
                    CustomerId = customer.CustomerId,

                    HubId = request.HubId,

                    DeliveryAddress = request.DeliveryAddress,

                    VoucherId = request.VoucherId,

                    OrderItems = items.Select(item => new OrderItem
                    {
                        ItemName = item.ItemName,

                        Quantity = item.Quantity,

                        UnitPrice = item.UnitPrice

                    }).ToList()
                };

                // Payment Strategy Pattern

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

                // Create order using business service

                await _orderService.CreateOrderAsync(
                    order,
                    request.DistanceKm,
                    request.QuietKitchenDiscount,
                    request.DeliveryFee,
                    request.RushHourSurcharge,
                    request.HeavyRainSurcharge,
                    paymentStrategy);

                // Clear cart and checkout session

                HttpContext.Session.Remove("Cart");

                HttpContext.Session.Remove(CheckoutKey);

                HttpContext.Session.Remove(
                    CheckoutKey + "UserId");

                // Redirect to complete page

                return RedirectToAction(
                    nameof(Complete),
                    new { id = order.OrderId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View("Confirm", request);
            }
        }

        // ==========================================
        // 11. ORDER COMPLETE
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
        // 12. ORDER STATUS
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
        // 13. ORDER DETAILS
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
        // 14. EDIT ORDER - GET
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
        // 15. EDIT ORDER - POST
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
                return BadRequest(
                    "Invalid order information.");
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
        // 16. UPDATE ORDER STATUS
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
                return BadRequest(
                    "Invalid order status.");
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
        // 17. CANCEL ORDER
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