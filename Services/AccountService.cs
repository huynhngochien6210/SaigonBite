
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
    // ==========================================
    // 1. DEPENDENCY INJECTION
    // ==========================================

    private readonly ApplicationDbContext _context;

    private readonly IPasswordHasher<UserAccount>
        _passwordHasher;

    public AccountService(
        ApplicationDbContext context,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }


    // ==========================================
    // 2. REGISTER LOCAL CUSTOMER
    // ==========================================

    public async Task<(bool Success, string Message, int UserId)>
        RegisterLocalAsync(RegisterLocalViewModel model)
    {
        // Validate input
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError, 0);
        }

        string phone = model.Phone.Trim();

        // Check duplicate phone number
        bool phoneExists = await _context.LocalCustomers
            .AnyAsync(c => c.PhoneNumber == phone);

        if (phoneExists)
        {
            return (
                false,
                "Phone number is already registered.",
                0
            );
        }

        // Create user account
        var user = new UserAccount
        {
            FullName = model.FullName.Trim(),
            Role = UserRole.Customer,
            AccountType = AccountType.LocalCustomer,
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            model.Password);

        // Create local customer profile
        user.LocalCustomer = new LocalCustomer
        {
            PhoneNumber = phone,
            UserAccount = user
        };

        // Save to database
        return await SaveAccountAsync(user);
    }


    // ==========================================
    // 3. REGISTER FOREIGN TOURIST
    // ==========================================

    public async Task<(bool Success, string Message, int UserId)>
        RegisterForeignAsync(RegisterForeignViewModel model)
    {
        // Validate input
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError, 0);
        }

        string email = model.Email
            .Trim()
            .ToLowerInvariant();

        string passport = model.PassportNumber
            .Trim()
            .ToUpperInvariant();

        // Check duplicate email
        bool emailExists = await _context.ForeignTourists
            .AnyAsync(c => c.Email == email);

        if (emailExists)
        {
            return (
                false,
                "Email is already registered.",
                0
            );
        }

        // Check duplicate passport number
        bool passportExists = await _context.ForeignTourists
            .AnyAsync(c => c.PassportNumber == passport);

        if (passportExists)
        {
            return (
                false,
                "Passport number is already registered.",
                0
            );
        }

        // Create user account
        var user = new UserAccount
        {
            FullName = model.FullName.Trim(),
            Role = UserRole.Customer,
            AccountType = AccountType.ForeignTourist,
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            model.Password);

        // Create foreign tourist profile
        user.ForeignTourist = new ForeignTourist
        {
            Email = email,
            PassportNumber = passport,
            Nationality = model.Nationality.Trim(),
            UserAccount = user
        };

        // Save to database
        return await SaveAccountAsync(user);
    }


    // ==========================================
    // 4. GET CUSTOMER PROFILE
    // ==========================================

    public async Task<AccountProfileViewModel?>
        GetProfileAsync(int userId)
    {
        // Find active and verified customer
        var user = await _context.UserAccounts
            .AsNoTracking()
            .Include(u => u.LocalCustomer)
            .Include(u => u.ForeignTourist)
            .FirstOrDefaultAsync(u =>
                u.UserId == userId &&
                u.Role == UserRole.Customer &&
                u.IsActive &&
                u.IsVerified);

        if (user == null)
        {
            return null;
        }

        // Common account information
        var profile = new AccountProfileViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            IsVerified = user.IsVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        // Local Customer profile
        if (
            user.AccountType == AccountType.LocalCustomer &&
            user.LocalCustomer != null
        )
        {
            profile.AccountType =
                AccountType.LocalCustomer;

            profile.PhoneNumber =
                user.LocalCustomer.PhoneNumber;

            profile.Email =
                user.LocalCustomer.Email;

            profile.DateOfBirth =
                user.LocalCustomer.DateOfBirth;
        }

        // Foreign Tourist profile
        else if (
            user.AccountType == AccountType.ForeignTourist &&
            user.ForeignTourist != null
        )
        {
            profile.AccountType =
                AccountType.ForeignTourist;

            profile.Email =
                user.ForeignTourist.Email;

            profile.PhoneNumber =
                user.ForeignTourist.PhoneNumber;

            profile.PassportNumber =
                user.ForeignTourist.PassportNumber;

            profile.Nationality =
                user.ForeignTourist.Nationality;

            profile.DateOfBirth =
                user.ForeignTourist.DateOfBirth;
        }

        // Invalid customer profile
        else
        {
            return null;
        }

        return profile;
    }


    // ==========================================
    // 5. UPDATE CUSTOMER PROFILE
    // ==========================================

    public async Task<(bool Success, string Message)>
        UpdateProfileAsync(
            int userId,
            EditAccountViewModel model)
    {
        // Validate input
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError);
        }

        // Validate full name
        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            return (
                false,
                "Full name is required."
            );
        }

        // Validate date of birth
        if (
            model.DateOfBirth.HasValue &&
            model.DateOfBirth.Value.Date > DateTime.Today
        )
        {
            return (
                false,
                "Date of birth cannot be in the future."
            );
        }

        // Find active and verified customer
        var user = await _context.UserAccounts
            .Include(u => u.LocalCustomer)
            .Include(u => u.ForeignTourist)
            .FirstOrDefaultAsync(u =>
                u.UserId == userId &&
                u.Role == UserRole.Customer &&
                u.IsActive &&
                u.IsVerified);

        if (user == null)
        {
            return (
                false,
                "Customer account not found."
            );
        }

        // Update Local Customer
        if (
            user.AccountType == AccountType.LocalCustomer &&
            user.LocalCustomer != null
        )
        {
            user.LocalCustomer.Email =
                string.IsNullOrWhiteSpace(model.Email)
                    ? null
                    : model.Email.Trim().ToLowerInvariant();

            user.LocalCustomer.DateOfBirth =
                model.DateOfBirth?.Date;

            // Login phone number cannot be changed
        }

        // Update Foreign Tourist
        else if (
            user.AccountType == AccountType.ForeignTourist &&
            user.ForeignTourist != null
        )
        {
            // Validate nationality
            if (string.IsNullOrWhiteSpace(model.Nationality))
            {
                return (
                    false,
                    "Nationality is required."
                );
            }

            user.ForeignTourist.Nationality =
                model.Nationality.Trim();

            user.ForeignTourist.PhoneNumber =
                string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? null
                    : model.PhoneNumber.Trim();

            user.ForeignTourist.DateOfBirth =
                model.DateOfBirth?.Date;

            // Login email and passport cannot be changed
        }

        // Invalid customer profile
        else
        {
            return (
                false,
                "Invalid customer profile."
            );
        }

        // Update common account information
        user.FullName = model.FullName.Trim();

        user.UpdatedAt = DateTime.UtcNow;

        // Save changes to database
        await _context.SaveChangesAsync();

        return (
            true,
            "Profile updated successfully."
        );
    }


    // ==========================================
    // 6. VALIDATE DATA ANNOTATIONS
    // ==========================================

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
            ?? "Invalid account information.";
    }


    // ==========================================
    // 7. SAVE ACCOUNT TO DATABASE
    // ==========================================

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
            when (
                ex.InnerException is SqlException sqlEx &&
                (
                    sqlEx.Number == 2601 ||
                    sqlEx.Number == 2627
                )
            )
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
