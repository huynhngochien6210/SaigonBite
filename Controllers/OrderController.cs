using Microsoft.AspNetCore.Mvc;

namespace SaigonBite.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Confirm()
        {
            return View();
        }

        public IActionResult Complete()
        {
            return View();
        }

        public IActionResult Status()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }

        public IActionResult IndexOrder()
        {
            return View("IndexOrder");
        }
    }
}