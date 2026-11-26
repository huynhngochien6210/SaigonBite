
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SaigonBite.ViewModels;

namespace SaigonBite.Controllers;

public class AdminController : Controller
{
    private readonly SaigonBite.Services.IAuthenticationService
        _authenticationService;

    public AdminController(
        SaigonBite.Services.IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    // Display Admin Login
    [HttpGet]
    public IActionResult Login()
    {
        return View(new AdminLoginViewModel());
    }

    // Process Admin Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        AdminLoginViewModel model)
    {
        // Validate input
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Authenticate admin
        var result = await _authenticationService
            .LoginAdminAsync(model);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        var user = result.User;

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // Create authentication cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        HttpContext.Session.Clear();

        // Temporary endpoint for testing
        return RedirectToAction(nameof(LoginSuccess));
    }

    // Temporary endpoint to verify Admin authorization
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult LoginSuccess()
    {
        return Content(
            $"Admin login successful. Welcome, {User.Identity?.Name}!");
    }
}
