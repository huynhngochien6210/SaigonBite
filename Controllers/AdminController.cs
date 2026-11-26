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
    }
}