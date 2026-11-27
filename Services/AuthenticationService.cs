using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Data;
using SaigonBite.Enums;
using SaigonBite.Models;
using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;

    private readonly IPasswordHasher<UserAccount>
        _passwordHasher;

    public AuthenticationService(
        ApplicationDbContext context,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // ==========================================
    // 1. CUSTOMER LOGIN
    // ==========================================

    public async Task<(bool Success,
                       string Message,
                       UserAccount? User)>
        LoginCustomerAsync(LoginViewModel model)
    {
        if (!IsValidModel(model))
        {
            return (false,
                "Invalid login information.", null);
        }

        string identifier = model.Identifier.Trim();

        UserAccount? user;

        // Foreign Tourist Login
        if (identifier.Contains('@'))
        {
            string email = identifier.ToLowerInvariant();

            user = await _context.UserAccounts
                .FirstOrDefaultAsync(u =>
                    u.Role == UserRole.Customer &&
                    u.AccountType == AccountType.ForeignTourist &&
                    u.ForeignTourist != null &&
                    u.ForeignTourist.Email == email);
        }
        // Local Customer Login
        else
        {
            user = await _context.UserAccounts
                .FirstOrDefaultAsync(u =>
                    u.Role == UserRole.Customer &&
                    u.AccountType == AccountType.LocalCustomer &&
                    u.LocalCustomer != null &&
                    u.LocalCustomer.PhoneNumber == identifier);
        }

        return await ValidateAccountAsync(
            user,
            model.Password,
            requireVerification: true);
    }

    // ==========================================
    // 2. ADMIN LOGIN
    // ==========================================

    public async Task<(bool Success,
                       string Message,
                       UserAccount? User)>
        LoginAdminAsync(AdminLoginViewModel model)
    {
        if (!IsValidModel(model))
        {
            return (false,
                "Invalid login information.", null);
        }

        string username = model.Username.Trim();

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u =>
                u.Username == username &&
                u.Role == UserRole.Admin);

        return await ValidateAccountAsync(
            user,
            model.Password,
            requireVerification: false);
    }

    // ==========================================
    // 3. SHARED ACCOUNT VALIDATION
    // ==========================================

    private async Task<(bool Success,
                        string Message,
                        UserAccount? User)>
        ValidateAccountAsync(
            UserAccount? user,
            string password,
            bool requireVerification)
    {
        // Account does not exist
        if (user == null)
        {
            return (false,
                "Invalid credentials.", null);
        }

        // Verify password hash
        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return (false,
                "Invalid credentials.", null);
        }

        // ======================================
        // CHECK ACCOUNT ACTIVATION STATUS
        // ======================================

        if (!user.IsActive)
        {
            // Customer voluntarily deactivated
            // their own account
            if (user.Role == UserRole.Customer &&
                user.IsVerified &&
                user.DeactivationReason ==
                    DeactivationReason.SelfDeactivated &&
                user.DeactivatedAt.HasValue &&
                (
                    user.AccountType ==
                        AccountType.LocalCustomer ||
                    user.AccountType ==
                        AccountType.ForeignTourist
                ))
            {
                return (
                    false,
                    "Account reactivation is required.",
                    user
                );
            }

            // Admin suspended or unknown reason
            return (
                false,
                "This account is unavailable. Please contact support.",
                null
            );
        }

        // Check customer verification status
        if (requireVerification && !user.IsVerified)
        {
            return (false,
                "Account verification is required.",
                user);
        }

        // Upgrade password hash if necessary
        if (passwordResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    password);

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return (
            true,
            "Login successful.",
            user
        );
    }

    // ==========================================
    // 4. SERVER-SIDE MODEL VALIDATION
    // ==========================================

    private static bool IsValidModel(object model)
    {
        var context = new ValidationContext(model);

        return Validator.TryValidateObject(
            model,
            context,
            new List<ValidationResult>(),
            validateAllProperties: true);
    }
}