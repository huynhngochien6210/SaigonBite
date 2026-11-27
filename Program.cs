using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using SaigonBite.Data;
using SaigonBite.Repositories;
using SaigonBite.Models;
using SaigonBite.Services;
using SaigonBite.Enums;
using SaigonBite.Security;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. MVC
// ==========================================

builder.Services.AddControllersWithViews();

// ==========================================
// 2. DATABASE
// ==========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// ==========================================
// 3. DEPENDENCY INJECTION
// ==========================================

// UC-D: Order

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<
    IOrderHistoryService,
    OrderHistoryService>();

// UC-D & UC-E Integration

builder.Services.AddScoped<
    ICustomerAccountService,
    CustomerAccountService>();

// UC-E: Account

builder.Services.AddScoped<
    IAccountService,
    AccountService>();

builder.Services.AddScoped<
    IVerificationService,
    VerificationService>();

builder.Services.AddScoped<
    IAccountReactivationService,
    AccountReactivationService>();

builder.Services.AddScoped<
    IPasswordResetService,
    PasswordResetService>();

builder.Services.AddScoped<
    SaigonBite.Services.IAuthenticationService,
    SaigonBite.Services.AuthenticationService>();

builder.Services.AddScoped<
    IPasswordHasher<UserAccount>,
    PasswordHasher<UserAccount>>();

// ==========================================
// 4. SESSION
// ==========================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});

// ==========================================
// 5. COOKIE AUTHENTICATION
// ==========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath = "/Account/Login";

        options.Cookie.Name = "SaigonBite.Auth";

        options.Cookie.HttpOnly = true;

        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.SlidingExpiration = true;

        // ==========================================
        // COOKIE VALIDATION - USE CASE E
        // ==========================================

        options.Events.OnValidatePrincipal =
            async context =>
        {
            var principal = context.Principal;

            if (principal == null)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

                return;
            }

            // Only validate Customer accounts.
            // Do not interfere with other team roles.

            if (!principal.IsInRole("Customer"))
            {
                return;
            }

            // Retrieve UserId.

            string? userIdClaim = principal
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            // Retrieve Security Stamp.

            string? securityStamp = principal
                .FindFirst(AuthClaimTypes.SecurityStamp)
                ?.Value;

            // Validate authentication claims.

            if (!int.TryParse(userIdClaim, out int userId)
                || userId <= 0
                || string.IsNullOrWhiteSpace(securityStamp))
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

                context.HttpContext.Session.Clear();

                return;
            }

            // Resolve database context.

            var dbContext = context.HttpContext
                .RequestServices
                .GetRequiredService<ApplicationDbContext>();

            // Retrieve current account state.

            var account = await dbContext.UserAccounts
                .AsNoTracking()
                .Where(u =>
                    u.UserId == userId &&
                    u.Role == UserRole.Customer &&
                    u.IsActive &&
                    u.IsVerified)
                .Select(u => new
                {
                    u.SecurityStamp
                })
                .SingleOrDefaultAsync(
                    context.HttpContext.RequestAborted);

            // Validate current account and Security Stamp.

            bool isValidAccount =
                account != null &&
                !string.IsNullOrWhiteSpace(
                    account.SecurityStamp) &&
                string.Equals(
                    account.SecurityStamp,
                    securityStamp,
                    StringComparison.Ordinal);

            // Reject outdated or invalid cookies.

            if (!isValidAccount)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

                context.HttpContext.Session.Clear();
            }
        };
    });

builder.Services.AddAuthorization();

// ==========================================
// 6. BUILD APPLICATION
// ==========================================

var app = builder.Build();

// ==========================================
// 7. ADMIN SEEDER
// ==========================================

if (app.Environment.IsDevelopment() &&
    args.Contains("--seed-admin"))
{
    await AdminSeeder.SeedAsync(app.Services);

    return;
}

// ==========================================
// 8. HTTP REQUEST PIPELINE
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// 9. ROUTING
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================================
// 10. RUN APPLICATION
// ==========================================

app.Run();