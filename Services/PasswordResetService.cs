
using System.Data;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SaigonBite.Data;
using SaigonBite.Enums;
using SaigonBite.Models;
using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly ApplicationDbContext _context;

    private readonly IPasswordHasher<UserAccount>
        _passwordHasher;

    private readonly IWebHostEnvironment _environment;

    private readonly byte[] _secretKey;

    private const int ExpirationMinutes = 5;
    private const int TokenExpirationMinutes = 10;
    private const int MaxFailedAttempts = 5;
    private const int ResendSeconds = 60;
    private const int MaxRequestsPerHour = 5;


    // ==========================================
    // 1. DEPENDENCY INJECTION
    // ==========================================

    public PasswordResetService(
        ApplicationDbContext context,
        IPasswordHasher<UserAccount> passwordHasher,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _environment = environment;

        string secretKey =
            configuration["Otp:SecretKey"]
            ?? throw new InvalidOperationException(
                "OTP secret key is not configured.");

        _secretKey = Convert.FromBase64String(secretKey);

        if (_secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "OTP secret key must contain at least 32 bytes.");
        }
    }


    // ==========================================
    // 2. REQUEST PASSWORD RESET
    // ==========================================

    public async Task<(bool Success, string Message, int? UserId)>
        RequestResetAsync(ForgotPasswordViewModel model)
    {
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError, null);
        }

        var query = _context.UserAccounts
            .AsNoTracking()
            .Where(u =>
                u.Role == UserRole.Customer &&
                u.IsActive &&
                u.IsVerified);

        UserAccount? user;

        if (model.RecoveryMethod == "local")
        {
            string phone = model.Phone!.Trim();

            user = await query.FirstOrDefaultAsync(u =>
                u.AccountType == AccountType.LocalCustomer &&
                u.LocalCustomer != null &&
                u.LocalCustomer.PhoneNumber == phone);
        }
        else
        {
            string email = model.Email!
                .Trim()
                .ToLowerInvariant();

            user = await query.FirstOrDefaultAsync(u =>
                u.AccountType == AccountType.ForeignTourist &&
                u.ForeignTourist != null &&
                u.ForeignTourist.Email == email);
        }

        if (user == null)
        {
            return (
                true,
                "If the account is eligible, a verification code will be generated.",
                null
            );
        }

        return (
            true,
            "Password reset requested.",
            user.UserId
        );
    }


    // ==========================================
    // 3. GENERATE PASSWORD RESET OTP
    // ==========================================

    public async Task<(bool Success, string Message)>
        SendCodeAsync(int userId)
    {
        if (!_environment.IsDevelopment())
        {
            return (
                false,
                "OTP delivery is not configured."
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var user = await FindEligibleUserAsync(userId);

        if (user == null)
        {
            return (false, "Invalid reset request.");
        }

        DateTime now = DateTime.UtcNow;

        var latestReset = await _context.PasswordResets
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.ResetId)
            .FirstOrDefaultAsync();

        // Resend cooldown
        if (latestReset != null &&
            now < latestReset.CreatedAt.AddSeconds(
                ResendSeconds))
        {
            return (
                false,
                "Please wait 60 seconds before requesting another code."
            );
        }

        // Limit requests per hour
        int recentRequests =
            await _context.PasswordResets.CountAsync(r =>
                r.UserId == userId &&
                r.CreatedAt >= now.AddHours(-1));

        if (recentRequests >= MaxRequestsPerHour)
        {
            return (
                false,
                "Too many password reset requests. Please try again later."
            );
        }

        // Invalidate previous requests
        var previousResets = await _context.PasswordResets
            .Where(r =>
                r.UserId == userId &&
                !r.IsUsed)
            .ToListAsync();

        foreach (var previous in previousResets)
        {
            previous.IsUsed = true;
        }

        // Generate secure six-digit OTP
        string otp = RandomNumberGenerator
            .GetInt32(0, 1000000)
            .ToString("D6");

        var reset = new PasswordReset
        {
            UserId = userId,
            CodeHash = string.Empty,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(ExpirationMinutes),
            IsUsed = false,
            FailedAttempts = 0
        };

        _context.PasswordResets.Add(reset);

        // Generate ResetId inside the transaction
        await _context.SaveChangesAsync();

        reset.CodeHash = HashOtp(
            userId,
            reset.ResetId,
            otp);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        // Development simulation only
        Console.WriteLine();
        Console.WriteLine("==============================");
        Console.WriteLine("SAIGONBITE PASSWORD RESET OTP");
        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"OTP Code: {otp}");
        Console.WriteLine("Expires in: 5 minutes");
        Console.WriteLine("==============================");
        Console.WriteLine();

        return (
            true,
            "Verification code generated successfully."
        );
    }


    // ==========================================
    // 4. VERIFY PASSWORD RESET OTP
    // ==========================================

    public async Task<(bool Success, string Message, string? Token)>
        VerifyCodeAsync(int userId, string code)
    {
        if (string.IsNullOrEmpty(code) ||
            !Regex.IsMatch(code, @"\A[0-9]{6}\z"))
        {
            return (
                false,
                "Please enter a valid 6-digit code.",
                null
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var user = await FindEligibleUserAsync(userId);

        if (user == null)
        {
            return (false, "Invalid reset request.", null);
        }

        var reset = await _context.PasswordResets
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.ResetId)
            .FirstOrDefaultAsync();

        if (reset == null)
        {
            return (
                false,
                "Verification code was not found.",
                null
            );
        }

        if (reset.FailedAttempts >= MaxFailedAttempts)
        {
            return (
                false,
                "Too many failed attempts. Request a new code.",
                null
            );
        }

        if (reset.IsUsed || reset.VerifiedAt.HasValue)
        {
            return (
                false,
                "Verification code is no longer valid.",
                null
            );
        }

        DateTime now = DateTime.UtcNow;

        if (reset.ExpiresAt <= now)
        {
            return (
                false,
                "Verification code has expired.",
                null
            );
        }

        string inputHash = HashOtp(
            userId,
            reset.ResetId,
            code);

        bool valid = SecureEquals(
            inputHash,
            reset.CodeHash);

        if (!valid)
        {
            reset.FailedAttempts++;

            int remaining =
                MaxFailedAttempts - reset.FailedAttempts;

            if (remaining <= 0)
            {
                reset.IsUsed = true;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (
                false,
                $"Incorrect verification code. Remaining attempts: {remaining}.",
                null
            );
        }

        // Generate secure 256-bit reset token
        string token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        reset.VerifiedAt = now;

        reset.TokenHash = HashToken(token);

        reset.TokenExpiresAt =
            now.AddMinutes(TokenExpirationMinutes);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return (
            true,
            "Verification successful.",
            token
        );
    }


    // ==========================================
    // 5. RESET PASSWORD
    // ==========================================

    public async Task<(bool Success, string Message)>
        ResetPasswordAsync(
            int userId,
            string token,
            ResetPasswordViewModel model)
    {
        string? validationError = ValidateModel(model);

        if (validationError != null)
        {
            return (false, validationError);
        }

        if (string.IsNullOrEmpty(token) ||
            token.Length != 64)
        {
            return (
                false,
                "Invalid password reset token."
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var user = await FindEligibleUserAsync(userId);

        if (user == null)
        {
            return (
                false,
                "Invalid reset request."
            );
        }

        string tokenHash = HashToken(token);

        var reset = await _context.PasswordResets
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.TokenHash == tokenHash &&
                !r.IsUsed);

        if (reset == null ||
            !reset.VerifiedAt.HasValue ||
            !reset.TokenExpiresAt.HasValue)
        {
            return (
                false,
                "Invalid or used password reset token."
            );
        }

        if (reset.TokenExpiresAt.Value <= DateTime.UtcNow)
        {
            return (
                false,
                "Password reset session has expired."
            );
        }

        // Prevent reuse of current password
        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.NewPassword);

        if (passwordResult !=
            PasswordVerificationResult.Failed)
        {
            return (
                false,
                "New password must differ from current password."
            );
        }

        // Hash new password
        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                model.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;

        // Invalidate all remaining reset requests
        var activeResets = await _context.PasswordResets
            .Where(r =>
                r.UserId == userId &&
                !r.IsUsed)
            .ToListAsync();

        foreach (var activeReset in activeResets)
        {
            activeReset.IsUsed = true;
        }

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return (
            true,
            "Password reset successfully."
        );
    }


    // ==========================================
    // 6. FIND ELIGIBLE CUSTOMER
    // ==========================================

    private Task<UserAccount?> FindEligibleUserAsync(
        int userId)
    {
        return _context.UserAccounts
            .FirstOrDefaultAsync(u =>
                u.UserId == userId &&
                u.Role == UserRole.Customer &&
                u.IsActive &&
                u.IsVerified);
    }


    // ==========================================
    // 7. HASH OTP
    // ==========================================

    private string HashOtp(
        int userId,
        int resetId,
        string code)
    {
        string value =
            $"password-reset:{userId}:{resetId}:{code}";

        using var hmac =
            new HMACSHA256(_secretKey);

        byte[] hash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToBase64String(hash);
    }


    // ==========================================
    // 8. HASH RESET TOKEN
    // ==========================================

    private static string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash);
    }


    // ==========================================
    // 9. SECURE HASH COMPARISON
    // ==========================================

    private static bool SecureEquals(
        string first,
        string second)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(first),
            Encoding.UTF8.GetBytes(second));
    }


    // ==========================================
    // 10. VALIDATE VIEW MODEL
    // ==========================================

    private static string? ValidateModel(object model)
    {
        var results = new List<ValidationResult>();

        bool valid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        return valid
            ? null
            : results.FirstOrDefault()?.ErrorMessage
                ?? "Invalid input.";
    }
}
