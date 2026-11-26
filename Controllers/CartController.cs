using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SaigonBite.Models;

namespace SaigonBite.Controllers
{
    public class CartController : Controller
    {
        private const string CartKey = "Cart";

        // =========================
        // HIỂN THỊ GIỎ HÀNG
        // =========================
        public IActionResult Index()
        {
            var cart = GetCart();

            ViewBag.Subtotal = cart.Sum(x => x.Total);

            return View(cart);
        }


        // =========================
        // THÊM MÓN
        // =========================
        [HttpPost]
        public IActionResult Add(
            string itemName,
            decimal unitPrice)
        {
            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(
                x => x.ItemName == itemName);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ItemName = itemName,
                    UnitPrice = unitPrice,
                    Quantity = 1
                });
            }

            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // TĂNG SỐ LƯỢNG
        // =========================
        [HttpPost]
        public IActionResult Increase(string itemName)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                x => x.ItemName == itemName);

            if (item != null)
            {
                item.Quantity++;
            }

            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // GIẢM SỐ LƯỢNG
        // =========================
        [HttpPost]
        public IActionResult Decrease(string itemName)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                x => x.ItemName == itemName);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // XÓA MÓN
        // =========================
        [HttpPost]
        public IActionResult Remove(string itemName)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                x => x.ItemName == itemName);

            if (item != null)
            {
                cart.Remove(item);
            }

            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // LẤY CART TỪ SESSION
        // =========================
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);

            if (string.IsNullOrEmpty(json))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(json)
                   ?? new List<CartItem>();
        }


        // =========================
        // LƯU CART VÀO SESSION
        // =========================
        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);

            HttpContext.Session.SetString(
                CartKey,
                json);
        }
    }
}
