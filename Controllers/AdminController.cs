using Microsoft.AspNetCore.Mvc;

namespace SaigonBite.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Trang tổng quan quản lý UC-A
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Brands()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CreateBrand()
        {
            return View();
        }
        [HttpGet]
        public IActionResult EditBrand(int id)
        {
            // Hiện tại sử dụng dữ liệu mẫu trên giao diện.
            return View();
        }
        [HttpGet]
        public IActionResult ViewBrand(int id)
        {
            //ViewBag.BrandId = id;
            return View();
        }

        [HttpGet]
        public IActionResult Foods()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ViewFood(int id)
        {
            ViewBag.FoodId = id;
            return View();
        }
        [HttpGet]
        public IActionResult EditFood(int id)
        {
            ViewBag.FoodId = id;
            return View();
        }
    }
}