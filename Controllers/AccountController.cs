using Microsoft.AspNetCore.Mvc;

namespace SaigonBite.Controllers;

public class AccountController : Controller
{
    [HttpGet("/Account/Login")]
    public IActionResult Login()
    {
        return View();
    }
}