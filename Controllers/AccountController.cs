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

    [HttpGet]
    public IActionResult RegistrationSuccess(string type = "local")
    {
        if (type != "local" && type != "foreign")
        {
            return RedirectToAction("Register");
        }

        ViewData["AccountType"] = type;

        return View();
    }

    [HttpGet]
    public IActionResult VerifyPhone()
    {
        return View();
    }

    [HttpGet]
    public IActionResult VerifyEmail()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpGet]
    public IActionResult VerifyResetCode(string type = "local")
    {
        if (type != "local" && type != "foreign")
        {
            return RedirectToAction("ForgotPassword");
        }

        ViewData["AccountType"] = type;
        ViewData["IsPasswordReset"] = true;

        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPasswordSuccess()
    {
        return View();
    }
}