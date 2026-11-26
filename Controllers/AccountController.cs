
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

using SaigonBite.Services;
using SaigonBite.ViewModels;
using SaigonBite.Enums;

namespace SaigonBite.Controllers;

public class AccountController : Controller
{
    // ==========================================
    // 1. DEPENDENCY INJECTION
    // ==========================================

    private readonly IAccountService _accountService;

    private readonly IVerificationService
        _verificationService;

    private readonly SaigonBite.Services.IAuthenticationService
        _authenticationService;

    private const string PendingUserIdKey =
        "PendingVerificationUserId";

    private const string PendingAccountTypeKey =
        "PendingAccountType";

    public AccountController(
        IAccountService accountService,
        IVerificationService verificationService,
        SaigonBite.Services.IAuthenticationService authenticationService)
    {
        _accountService = accountService;
        _verificationService = verificationService;
        _authenticationService = authenticationService;
    }


    // ==========================================
    // 2. CUSTOMER LOGIN
    // ==========================================

    [HttpGet("/Account/Login")]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost("/Account/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService
            .LoginCustomerAsync(model);

        if (!result.Success)
        {
            // Correct password but unverified account
            if (result.User != null &&
                result.User.IsActive &&
                !result.User.IsVerified)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                string accountType =
                    result.User.AccountType ==
                    AccountType.LocalCustomer
                        ? "local"
                        : "foreign";

                HttpContext.Session.SetInt32(
                    PendingUserIdKey,
                    result.User.UserId);

                HttpContext.Session.SetString(
                    PendingAccountTypeKey,
                    accountType);

                return RedirectToAction(
                    nameof(RegistrationSuccess));
            }

            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        var user = result.User!;

        // Create authentication claims
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
                user.Role.ToString()),

            new Claim(
                "AccountType",
                user.AccountType?.ToString() ?? "")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        // Clear pending verification session
        HttpContext.Session.Remove(PendingUserIdKey);

        HttpContext.Session.Remove(PendingAccountTypeKey);

        return RedirectToAction(nameof(MyAccount));
    }


    // ==========================================
    // 3. SELECT ACCOUNT TYPE
    // ==========================================

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }


    // ==========================================
    // 4. LOCAL CUSTOMER REGISTRATION
    // ==========================================

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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService
            .RegisterLocalAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        HttpContext.Session.SetInt32(
            PendingUserIdKey,
            result.UserId);

        HttpContext.Session.SetString(
            PendingAccountTypeKey,
            "local");

        return RedirectToAction(
            nameof(RegistrationSuccess),
            new { type = "local" });
    }


    // ==========================================
    // 5. FOREIGN TOURIST REGISTRATION
    // ==========================================

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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService
            .RegisterForeignAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        HttpContext.Session.SetInt32(
            PendingUserIdKey,
            result.UserId);

        HttpContext.Session.SetString(
            PendingAccountTypeKey,
            "foreign");

        return RedirectToAction(
            nameof(RegistrationSuccess),
            new { type = "foreign" });
    }


    // ==========================================
    // 6. REGISTRATION SUCCESS
    // ==========================================

    [HttpGet]
    public IActionResult RegistrationSuccess(
        string type = "local")
    {
        int? userId = HttpContext.Session
            .GetInt32(PendingUserIdKey);

        string? accountType = HttpContext.Session
            .GetString(PendingAccountTypeKey);

        if (userId == null ||
            (accountType != "local" &&
             accountType != "foreign"))
        {
            return RedirectToAction(nameof(Register));
        }

        ViewData["AccountType"] = accountType;

        return View();
    }


    // ==========================================
    // 7. PHONE VERIFICATION
    // ==========================================

    [HttpGet]
    public IActionResult VerifyPhone()
    {
        int? userId = HttpContext.Session
            .GetInt32(PendingUserIdKey);

        string? accountType = HttpContext.Session
            .GetString(PendingAccountTypeKey);

        if (userId == null ||
            accountType != "local")
        {
            return RedirectToAction(nameof(Register));
        }

        ViewData["AccountType"] = "local";

        return View();
    }


    // ==========================================
    // 8. EMAIL VERIFICATION
    // ==========================================

    [HttpGet]
    public IActionResult VerifyEmail()
    {
        int? userId = HttpContext.Session
            .GetInt32(PendingUserIdKey);

        string? accountType = HttpContext.Session
            .GetString(PendingAccountTypeKey);

        if (userId == null ||
            accountType != "foreign")
        {
            return RedirectToAction(nameof(Register));
        }

        ViewData["AccountType"] = "foreign";

        return View();
    }


    // ==========================================
    // 9. SEND VERIFICATION CODE
    // ==========================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVerificationCode()
    {
        int? userId = HttpContext.Session
            .GetInt32(PendingUserIdKey);

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Verification session has expired."
            });
        }

        var result = await _verificationService
            .SendCodeAsync(userId.Value);

        return Json(new
        {
            success = result.Success,
            message = result.Message
        });
    }


    // ==========================================
    // 10. VERIFY REGISTRATION CODE
    // ==========================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyRegistrationCode(
        string code)
    {
        int? userId = HttpContext.Session
            .GetInt32(PendingUserIdKey);

        if (userId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Verification session has expired."
            });
        }

        var result = await _verificationService
            .VerifyCodeAsync(userId.Value, code);

        if (!result.Success)
        {
            return Json(new
            {
                success = false,
                message = result.Message
            });
        }

        HttpContext.Session.Remove(PendingUserIdKey);

        HttpContext.Session.Remove(PendingAccountTypeKey);

        return Json(new
        {
            success = true,
            message = result.Message,
            redirectUrl = Url.Action(
                nameof(Login),
                "Account")
        });
    }


    // ==========================================
    // 11. FORGOT PASSWORD
    // ==========================================

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }


    // ==========================================
    // 12. VERIFY PASSWORD RESET CODE
    // ==========================================

    [HttpGet]
    public IActionResult VerifyResetCode(
        string type = "local")
    {
        if (type != "local" &&
            type != "foreign")
        {
            return RedirectToAction(
                nameof(ForgotPassword));
        }

        ViewData["AccountType"] = type;

        ViewData["IsPasswordReset"] = true;

        return View();
    }


    // ==========================================
    // 13. RESET PASSWORD
    // ==========================================

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


    // ==========================================
    // 14. VIEW ACCOUNT
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyAccount()
    {
        // Get authenticated customer ID
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        // Retrieve profile from database
        var profile = await _accountService
            .GetProfileAsync(userId.Value);

        if (profile == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Login));
        }

        return View(profile);
    }


    // ==========================================
    // 15. EDIT ACCOUNT - GET
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> EditAccount()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var profile = await _accountService
            .GetProfileAsync(userId.Value);

        if (profile == null)
        {
            return RedirectToAction(nameof(MyAccount));
        }

        // Prepare editable fields
        var model = new EditAccountViewModel
        {
            FullName = profile.FullName,

            Email =
                profile.AccountType ==
                AccountType.LocalCustomer
                    ? profile.Email
                    : null,

            PhoneNumber =
                profile.AccountType ==
                AccountType.ForeignTourist
                    ? profile.PhoneNumber
                    : null,

            DateOfBirth = profile.DateOfBirth,

            Nationality = profile.Nationality
        };

        // Store original profile for read-only fields
        ViewData["Profile"] = profile;

        return View(model);
    }


    // ==========================================
    // 16. EDIT ACCOUNT - POST
    // ==========================================

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(
        EditAccountViewModel model)
    {
        // Get authenticated customer ID
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        // Retrieve current profile
        var profile = await _accountService
            .GetProfileAsync(userId.Value);

        if (profile == null)
        {
            return RedirectToAction(nameof(MyAccount));
        }

        ViewData["Profile"] = profile;

        // Validate input
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Update customer profile
        var result = await _accountService
            .UpdateProfileAsync(userId.Value, model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        // Retrieve current authentication properties
        var authentication =
            await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

        // Update FullName inside authentication cookie
        if (authentication.Succeeded &&
            User.Identity is ClaimsIdentity identity)
        {
            var nameClaims = identity
                .FindAll(ClaimTypes.Name)
                .ToList();

            foreach (var claim in nameClaims)
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaim(
                new Claim(
                    ClaimTypes.Name,
                    model.FullName.Trim()));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                User,
                authentication.Properties);
        }

        // Success message after redirect
        TempData["ProfileSuccess"] =
            "Profile updated successfully.";

        return RedirectToAction(nameof(MyAccount));
    }


    // ==========================================
    // 17. CHANGE PASSWORD
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public IActionResult ChangePassword()
    {
        return View();
    }


    // ==========================================
    // 18. ORDER HISTORY
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public IActionResult OrderHistory()
    {
        return View();
    }


    // ==========================================
    // 19. LOGOUT
    // ==========================================

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        bool isAdmin = User.IsInRole("Admin");

        // Remove authentication cookie
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        // Clear session
        HttpContext.Session.Clear();

        // Redirect based on user role
        if (isAdmin)
        {
            return RedirectToAction(
                "Login",
                "Admin");
        }

        return RedirectToAction(nameof(Login));
    }


    // ==========================================
    // 20. GET CURRENT USER ID
    // ==========================================

    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id) &&
               id > 0
            ? id
            : null;
    }
}
