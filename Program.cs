using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

using SaigonBite.Data;
using SaigonBite.Repositories;
using SaigonBite.Models;
using SaigonBite.Services;

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

// ---------- UC-D: Order ----------

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<OrderService>();

// ---------- UC-E: Account ----------

builder.Services.AddScoped<
    IAccountService,
    AccountService>();

builder.Services.AddScoped<
    IVerificationService,
    VerificationService>();

builder.Services.AddScoped<
    IAuthenticationService,
    AuthenticationService>();

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