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
    }
}