using System.Globalization;
using SaigonBite.Security;
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
    private readonly IAccountReactivationService _reactivationService;
    private const string ReactivationUserKey = "ReactivationUserId";
    private const string ReactivationDateKey = "ReactivationDate";
    private const string ReactivationExpiryKey = "ReactivationExpiry";
    private readonly IOrderHistoryService _orderHistoryService;
    private readonly IPasswordResetService _passwordResetService;

    private const string ResetUserIdKey =
        "PasswordResetUserId";

    private const string ResetAccountTypeKey =
        "PasswordResetAccountType";

    private const string ResetTokenKey =
        "PasswordResetToken";
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
        SaigonBite.Services.IAuthenticationService authenticationService,
        IPasswordResetService passwordResetService,
        IOrderHistoryService orderHistoryService,
        IAccountReactivationService reactivationService)
    {
        _accountService = accountService;
        _verificationService = verificationService;
        _authenticationService = authenticationService;
        _passwordResetService = passwordResetService;
        _orderHistoryService = orderHistoryService;
        _reactivationService = reactivationService;
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
        ClearReactivationSession();
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService
            .LoginCustomerAsync(model);

        if (!result.Success)
        {
            // Correct password for a voluntarily deactivated customer.
            var inactive = result.User;
            if (inactive != null && !inactive.IsActive && inactive.IsVerified &&
                inactive.Role == UserRole.Customer &&
                inactive.DeactivationReason == DeactivationReason.SelfDeactivated &&
                inactive.DeactivatedAt.HasValue &&
                (inactive.AccountType == AccountType.LocalCustomer ||
                 inactive.AccountType == AccountType.ForeignTourist))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32(ReactivationUserKey, inactive.UserId);
                HttpContext.Session.SetString(ReactivationDateKey,
                    inactive.DeactivatedAt.Value.Ticks.ToString(CultureInfo.InvariantCulture));
                HttpContext.Session.SetString(ReactivationExpiryKey,
                    DateTime.UtcNow.AddMinutes(15).Ticks.ToString(CultureInfo.InvariantCulture));
                return RedirectToAction(nameof(ReactivateAccount));
            }

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
                user.AccountType?.ToString() ?? ""),
            new Claim(AuthClaimTypes.SecurityStamp, user.SecurityStamp)
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

        return LocalRedirect("~/Home/Index");
    }


    // ACCOUNT REACTIVATION
    private void ClearReactivationSession()
    {
        HttpContext.Session.Remove(ReactivationUserKey);
        HttpContext.Session.Remove(ReactivationDateKey);
        HttpContext.Session.Remove(ReactivationExpiryKey);
    }

    private bool TryGetReactivation(out int userId, out DateTime deactivatedAt)
    {
        userId = HttpContext.Session.GetInt32(ReactivationUserKey) ?? 0;
        deactivatedAt = default;
        if (User.Identity?.IsAuthenticated == true || userId <= 0 ||
            !long.TryParse(HttpContext.Session.GetString(ReactivationDateKey), out long ticks) ||
            ticks <= 0 || ticks > DateTime.MaxValue.Ticks ||
            !long.TryParse(HttpContext.Session.GetString(ReactivationExpiryKey), out long expiry) ||
            expiry <= DateTime.UtcNow.Ticks)
        {
            ClearReactivationSession();
            return false;
        }
        deactivatedAt = new DateTime(ticks, DateTimeKind.Utc);
        return true;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ReactivateAccount()
    {
        if (!TryGetReactivation(out _, out _)) return RedirectToAction(nameof(Login));
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendReactivationCode()
    {
        if (!TryGetReactivation(out int userId, out DateTime date))
            return RedirectToAction(nameof(Login));
        var result = await _reactivationService.SendCodeAsync(userId, date);
        TempData["ReactivationMessage"] = result.Message;
        TempData["ReactivationSent"] = result.Success;
        return RedirectToAction(nameof(VerifyReactivation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyReactivation()
    {
        if (!TryGetReactivation(out _, out _)) return RedirectToAction(nameof(Login));
        return View(new VerifyReactivationViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyReactivation(VerifyReactivationViewModel model)
    {
        if (!TryGetReactivation(out int userId, out DateTime date))
            return RedirectToAction(nameof(Login));
        if (!ModelState.IsValid) return View(model);
        var result = await _reactivationService.VerifyCodeAsync(userId, date, model.Code);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        TempData["ReactivationSuccess"] = "Account reactivated successfully. Please sign in again.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult CancelReactivation()
    {
        ClearReactivationSession();
        return RedirectToAction(nameof(Login));
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
        ClearReactivationSession();
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
        ClearReactivationSession();
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
    // 11. FORGOT PASSWORD - GET
    // ==========================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        ClearPasswordResetSession();

        return View(new ForgotPasswordViewModel());
    }


    // ==========================================
    // 11. FORGOT PASSWORD - POST
    // ==========================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model)
    {
        ClearReactivationSession();
        ClearPasswordResetSession();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _passwordResetService
            .RequestResetAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        // Store recovery method in session
        HttpContext.Session.SetString(
            ResetAccountTypeKey,
            model.RecoveryMethod);

        // Store account ID only when eligible
        if (result.UserId.HasValue)
        {
            HttpContext.Session.SetInt32(
                ResetUserIdKey,
                result.UserId.Value);
        }

        return RedirectToAction(
            nameof(VerifyResetCode));
    }


    // ==========================================
    // 12. VERIFY RESET CODE - GET
    // ==========================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyResetCode()
    {
        string? accountType = HttpContext.Session
            .GetString(ResetAccountTypeKey);

        if (accountType != "local" &&
            accountType != "foreign")
        {
            return RedirectToAction(
                nameof(ForgotPassword));
        }

        if (!string.IsNullOrEmpty(
            HttpContext.Session.GetString(ResetTokenKey)))
        {
            return RedirectToAction(
                nameof(ResetPassword));
        }

        ViewData["AccountType"] = accountType;

        ViewData["IsPasswordReset"] = true;

        return View();
    }


    // ==========================================
    // 12. SEND PASSWORD RESET OTP
    // ==========================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendResetCode()
    {
        string? accountType = HttpContext.Session
            .GetString(ResetAccountTypeKey);

        if (accountType != "local" &&
            accountType != "foreign")
        {
            return Unauthorized(new
            {
                success = false,
                message = "Password reset session has expired."
            });
        }

        if (!string.IsNullOrEmpty(
            HttpContext.Session.GetString(ResetTokenKey)))
        {
            return Json(new
            {
                success = false,
                message = "Verification has already been completed."
            });
        }

        int? userId = HttpContext.Session
            .GetInt32(ResetUserIdKey);

        if (userId.HasValue)
        {
            await _passwordResetService
                .SendCodeAsync(userId.Value);
        }

        // Generic response to reduce account enumeration
        return Json(new
        {
            success = true,
            message = "If the account is eligible, a verification code will be generated."
        });
    }


    // ==========================================
    // 12. VERIFY PASSWORD RESET OTP - POST
    // ==========================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyResetCode(
        string code)
    {
        string? accountType = HttpContext.Session
            .GetString(ResetAccountTypeKey);

        if (accountType != "local" &&
            accountType != "foreign")
        {
            return Unauthorized(new
            {
                success = false,
                message = "Password reset session has expired."
            });
        }

        int? userId = HttpContext.Session
            .GetInt32(ResetUserIdKey);

        if (!userId.HasValue)
        {
            return Json(new
            {
                success = false,
                message = "Invalid or expired verification code."
            });
        }

        var result = await _passwordResetService
            .VerifyCodeAsync(userId.Value, code);

        if (!result.Success ||
            string.IsNullOrEmpty(result.Token))
        {
            return Json(new
            {
                success = false,
                message = "Invalid or expired verification code."
            });
        }

        // Store reset token on the server
        HttpContext.Session.SetString(
            ResetTokenKey,
            result.Token);

        return Json(new
        {
            success = true,
            message = "Verification successful.",
            redirectUrl = Url.Action(
                nameof(ResetPassword),
                "Account")
        });
    }


    // ==========================================
    // 13. RESET PASSWORD - GET
    // ==========================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword()
    {
        int? userId = HttpContext.Session
            .GetInt32(ResetUserIdKey);

        string? token = HttpContext.Session
            .GetString(ResetTokenKey);

        if (!userId.HasValue ||
            string.IsNullOrEmpty(token))
        {
            return RedirectToAction(
                nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel());
    }


    // ==========================================
    // 13. RESET PASSWORD - POST
    // ==========================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model)
    {
        int? userId = HttpContext.Session
            .GetInt32(ResetUserIdKey);

        string? token = HttpContext.Session
            .GetString(ResetTokenKey);

        if (!userId.HasValue ||
            string.IsNullOrEmpty(token))
        {
            return RedirectToAction(
                nameof(ForgotPassword));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _passwordResetService
            .ResetPasswordAsync(
                userId.Value,
                token,
                model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        // Remove current authentication cookie
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        // Clear all current session data
        HttpContext.Session.Clear();

        TempData["PasswordResetSuccess"] = true;

        return RedirectToAction(
            nameof(ResetPasswordSuccess));
    }


    // ==========================================
    // 13. RESET PASSWORD SUCCESS
    // ==========================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordSuccess()
    {
        if (TempData["PasswordResetSuccess"] is not true)
        {
            return RedirectToAction(nameof(Login));
        }

        return View();
    }


    // ==========================================
    // CLEAR PASSWORD RESET SESSION
    // ==========================================

    private void ClearPasswordResetSession()
    {
        HttpContext.Session.Remove(ResetUserIdKey);

        HttpContext.Session.Remove(ResetAccountTypeKey);

        HttpContext.Session.Remove(ResetTokenKey);
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
// 17. CHANGE PASSWORD - GET
// ==========================================

[HttpGet]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> ChangePassword()
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

    return View(new ChangePasswordViewModel());
}


    // ==========================================
    // 17. CHANGE PASSWORD - POST
    // ==========================================

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model)
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

        // Validate input
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Execute business logic
        var result = await _accountService
            .ChangePasswordAsync(userId.Value, model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        // Remove current authentication cookie
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        // Clear current session
        HttpContext.Session.Clear();

        // Success message
        TempData["PasswordSuccess"] =
            "Password changed successfully. Please sign in again.";

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeactivateAccount()
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

        return View(new DeactivateAccountViewModel());
    }
[HttpPost]
[Authorize(Roles = "Customer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeactivateAccount(
    DeactivateAccountViewModel model)
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

    // Validate input
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Deactivate customer account
    var result = await _accountService
        .DeactivateAccountAsync(userId.Value, model);

    if (!result.Success)
    {
        ModelState.AddModelError(
            string.Empty,
            result.Message);

        return View(model);
    }

    // Remove authentication cookie
    await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

    // Clear session
    HttpContext.Session.Clear();

    // Success notification
    TempData["DeactivateSuccess"] = result.Message;

    // Redirect to login page
    return RedirectToAction(nameof(Login));
}
    // ==========================================
    // 18. ORDER HISTORY
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> OrderHistory()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var model = await _orderHistoryService
            .GetOrderHistoryAsync(userId.Value);

        return View(model);
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
