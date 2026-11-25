using Microsoft.AspNetCore.Mvc;

namespace SaigonBite.Controllers;

public class AccountController : Controller
{
    [HttpGet("/Account/Login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult RegisterLocal()
    {
        return View();
    }

    [HttpGet]
    public IActionResult RegisterForeign()
    {
        return View();
    }
}