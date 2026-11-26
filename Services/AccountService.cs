
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Data;
using SaigonBite.Enums;
using SaigonBite.Models;
using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public AccountService(
        ApplicationDbContext context,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // Register Local Customer
    public async Task<(bool Success, string Message, int UserId)>
        RegisterLocalAsync(RegisterLocalViewModel model)
    {
        // 1. Validate input
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError, 0);
        }

        string phone = model.Phone.Trim();

        // 2. Check duplicate phone number
        bool phoneExists = await _context.LocalCustomers
            .AnyAsync(c => c.PhoneNumber == phone);

        if (phoneExists)
        {
            return (false,
                "Phone number is already registered.", 0);
        }

        // 3. Create user account
        var user = new UserAccount
        {
            FullName = model.FullName.Trim(),
            Role = UserRole.Customer,
            AccountType = AccountType.LocalCustomer,
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Hash password
        user.PasswordHash = _passwordHasher.HashPassword(
            user, model.Password);

        // 5. Create local customer profile
        user.LocalCustomer = new LocalCustomer
        {
            PhoneNumber = phone,
            UserAccount = user
        };

        // 6. Save to database
        return await SaveAccountAsync(user);
    }

    // Register Foreign Tourist
    public async Task<(bool Success, string Message, int UserId)>
        RegisterForeignAsync(RegisterForeignViewModel model)
    {
        // 1. Validate input
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError, 0);
        }

        string email = model.Email.Trim().ToLowerInvariant();

        string passport = model.PassportNumber
            .Trim()
            .ToUpperInvariant();

        // 2. Check duplicate email
        bool emailExists = await _context.ForeignTourists
            .AnyAsync(c => c.Email == email);

        if (emailExists)
        {
            return (false,
                "Email is already registered.", 0);
        }

        // 3. Check duplicate passport number
        bool passportExists = await _context.ForeignTourists
            .AnyAsync(c => c.PassportNumber == passport);

        if (passportExists)
        {
            return (false,
                "Passport number is already registered.", 0);
        }

        // 4. Create user account
        var user = new UserAccount
        {
            FullName = model.FullName.Trim(),
            Role = UserRole.Customer,
            AccountType = AccountType.ForeignTourist,
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Hash password
        user.PasswordHash = _passwordHasher.HashPassword(
            user, model.Password);

        // 6. Create foreign tourist profile
        user.ForeignTourist = new ForeignTourist
        {
            Email = email,
            PassportNumber = passport,
            Nationality = model.Nationality.Trim(),
            UserAccount = user
        };

        // 7. Save to database
        return await SaveAccountAsync(user);
    }

    // Validate DataAnnotations
    private static string? ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        if (isValid)
        {
            return null;
        }

        return results.FirstOrDefault()?.ErrorMessage
            ?? "Invalid registration information.";
    }

    // Save user account and customer profile
    private async Task<(bool Success, string Message, int UserId)>
        SaveAccountAsync(UserAccount user)
    {
        try
        {
            _context.UserAccounts.Add(user);

            await _context.SaveChangesAsync();

            return (
                true,
                "Registration successful.",
                user.UserId
            );
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx
                && (sqlEx.Number == 2601
                    || sqlEx.Number == 2627))
        {
            _context.ChangeTracker.Clear();

            return (
                false,
                "Account information is already registered.",
                0
            );
        }
    }
}
