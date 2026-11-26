using Microsoft.AspNetCore.Mvc;
using SaigonBite.Services;
using SaigonBite.ViewModels;
namespace SaigonBite.Controllers;

public class AccountController : Controller
{

    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

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


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterLocal(
        RegisterLocalViewModel model)
    {
        ViewData["AccountType"] = "local";

        // 1. Validate input
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 2. Call AccountService
        var result = await _accountService
            .RegisterLocalAsync(model);

        // 3. Handle registration failure
        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        // 4. Redirect after successful registration
        return RedirectToAction(
            nameof(RegistrationSuccess),
            new { type = "local" });
    }


    [HttpGet]
    public IActionResult RegisterForeign()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterForeign(
        RegisterForeignViewModel model)
    {
        ViewData["AccountType"] = "foreign";

        // 1. Validate input
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 2. Call AccountService
        var result = await _accountService
            .RegisterForeignAsync(model);

        // 3. Handle registration failure
        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        // 4. Redirect after successful registration
        return RedirectToAction(
            nameof(RegistrationSuccess),
            new { type = "foreign" });
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

    [HttpGet]
    public IActionResult MyAccount()
    {
        return View();
    }

    [HttpGet]
    public IActionResult EditAccount()
    {
        return View();
    }
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }
    [HttpGet]
    public IActionResult OrderHistory()
    {
        return View();
    }

    
}