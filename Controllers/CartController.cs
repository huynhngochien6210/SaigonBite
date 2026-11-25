using Microsoft.AspNetCore.Mvc;

namespace SaigonBite.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}