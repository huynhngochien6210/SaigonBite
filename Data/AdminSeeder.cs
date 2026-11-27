
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Enums;
using SaigonBite.Models;

namespace SaigonBite.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<UserAccount>>();

        string? username =
            configuration["SeedAdmin:Username"];

        string? fullName =
            configuration["SeedAdmin:FullName"];

        string? password =
            configuration["SeedAdmin:Password"];

        // Skip if credentials are not configured
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (password.Length < 12 ||
            password.Length > 128)
        {
            throw new InvalidOperationException(
                "Admin password must contain 12-128 characters.");
        }

        username = username.Trim();

        // Check existing username
        var existingUser = await context.UserAccounts
            .FirstOrDefaultAsync(u =>
                u.Username == username);

        if (existingUser != null)
        {
            if (existingUser.Role != UserRole.Admin)
            {
                throw new InvalidOperationException(
                    "Admin username is already in use.");
            }

            Console.WriteLine(
                "Admin account already exists.");

            return;
        }

        var admin = new UserAccount
        {
            FullName = fullName.Trim(),
            Username = username,
            Role = UserRole.Admin,
            AccountType = null,
            IsVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        admin.PasswordHash =
            passwordHasher.HashPassword(admin, password);

        context.UserAccounts.Add(admin);

        await context.SaveChangesAsync();

        Console.WriteLine(
            "Admin account created successfully.");
    }
}
