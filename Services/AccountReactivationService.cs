using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Data;
using SaigonBite.Enums;
using SaigonBite.Models;

namespace SaigonBite.Services;

public class AccountReactivationService
    : IAccountReactivationService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly byte[] _secretKey;

    private const int ExpirationMinutes = 5;
    private const int MaxFailedAttempts = 5;
    private const int ResendSeconds = 60;
    private const int MaxRequestsPerHour = 5;

    // ==========================================
    // 1. DEPENDENCY INJECTION
    // ==========================================

    public AccountReactivationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _context = context;
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
    // 2. CHECK REACTIVATION ELIGIBILITY
    // ==========================================

    private static bool CanReactivate(
        UserAccount? user,
        DateTime deactivatedAt)
    {
        return user != null &&
               user.Role == UserRole.Customer &&
               !user.IsActive &&
               user.IsVerified &&
               user.DeactivationReason ==
                   DeactivationReason.SelfDeactivated &&
               user.DeactivatedAt == deactivatedAt &&
               (
                   user.AccountType == AccountType.LocalCustomer ||
                   user.AccountType == AccountType.ForeignTourist
               );
    }

    // ==========================================
    // 3. GENERATE REACTIVATION OTP
    // ==========================================

    public async Task<(bool Success, string Message)>
        SendCodeAsync(
            int userId,
            DateTime deactivatedAt)
    {
        // Development simulation only
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

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (!CanReactivate(user, deactivatedAt))
        {
            return (
                false,
                "This account is not eligible for reactivation."
            );
        }

        DateTime now = DateTime.UtcNow;

        // Find latest reactivation OTP
        var latestCode = await _context.VerificationCodes
            .Where(v =>
                v.UserId == userId &&
                v.Purpose == VerificationPurpose.Reactivation)
            .OrderByDescending(v => v.CreatedAt)
            .ThenByDescending(v => v.VerificationId)
            .FirstOrDefaultAsync();

        // Resend cooldown
        if (latestCode != null &&
            now < latestCode.CreatedAt.AddSeconds(ResendSeconds))
        {
            return (
                false,
                "Please wait 60 seconds before requesting another code."
            );
        }

        // Limit OTP requests per hour
        int recentRequests =
            await _context.VerificationCodes.CountAsync(v =>
                v.UserId == userId &&
                v.Purpose == VerificationPurpose.Reactivation &&
                v.CreatedAt >= now.AddHours(-1));

        if (recentRequests >= MaxRequestsPerHour)
        {
            return (
                false,
                "Too many reactivation requests. Please try again later."
            );
        }

        // Invalidate previous reactivation codes
        var oldCodes = await _context.VerificationCodes
            .Where(v =>
                v.UserId == userId &&
                v.Purpose == VerificationPurpose.Reactivation &&
                !v.IsUsed)
            .ToListAsync();

        foreach (var oldCode in oldCodes)
        {
            oldCode.IsUsed = true;
        }

        // Generate secure six-digit OTP
        string otp = RandomNumberGenerator
            .GetInt32(0, 1000000)
            .ToString("D6");

        var verificationCode = new VerificationCode
        {
            UserId = userId,

            Purpose = VerificationPurpose.Reactivation,

            CodeHash = HashCode(
                userId,
                deactivatedAt,
                otp),

            CreatedAt = now,

            ExpiresAt = now.AddMinutes(ExpirationMinutes),

            IsUsed = false,

            FailedAttempts = 0
        };

        _context.VerificationCodes.Add(verificationCode);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        // Simulated OTP delivery
        Console.WriteLine();
        Console.WriteLine("==================================");
        Console.WriteLine("SAIGONBITE REACTIVATION OTP");
        Console.WriteLine("Purpose: Reactivation");
        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"OTP Code: {otp}");
        Console.WriteLine("Expires in: 5 minutes");
        Console.WriteLine("==================================");
        Console.WriteLine();

        return (
            true,
            "Reactivation code generated successfully."
        );
    }

    // ==========================================
    // 4. VERIFY REACTIVATION OTP
    // ==========================================

    public async Task<(bool Success, string Message)>
        VerifyCodeAsync(
            int userId,
            DateTime deactivatedAt,
            string code)
    {
        // Validate OTP format
        if (string.IsNullOrEmpty(code) ||
            !Regex.IsMatch(code, @"\A[0-9]{6}\z"))
        {
            return (
                false,
                "Please enter a valid 6-digit code."
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == userId);

        // Recheck account eligibility
        if (!CanReactivate(user, deactivatedAt))
        {
            return (
                false,
                "This account is not eligible for reactivation."
            );
        }

        // Find latest reactivation OTP
        var verificationCode =
            await _context.VerificationCodes
                .Where(v =>
                    v.UserId == userId &&
                    v.Purpose == VerificationPurpose.Reactivation &&
                    v.CreatedAt >= deactivatedAt)
                .OrderByDescending(v => v.CreatedAt)
                .ThenByDescending(v => v.VerificationId)
                .FirstOrDefaultAsync();

        if (verificationCode == null)
        {
            return (
                false,
                "Reactivation code was not found."
            );
        }

        // Check failed attempts
        if (verificationCode.FailedAttempts >= MaxFailedAttempts)
        {
            return (
                false,
                "Too many failed attempts. Request a new code."
            );
        }

        // Check whether OTP has been used
        if (verificationCode.IsUsed)
        {
            return (
                false,
                "Reactivation code is no longer valid."
            );
        }

        // Check expiration
        if (verificationCode.ExpiresAt <= DateTime.UtcNow)
        {
            return (
                false,
                "Reactivation code has expired."
            );
        }

        // Hash submitted OTP
        string inputHash = HashCode(
            userId,
            deactivatedAt,
            code);

        // Secure hash comparison
        bool isValid =
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(inputHash),
                Encoding.UTF8.GetBytes(
                    verificationCode.CodeHash));

        // Incorrect OTP
        if (!isValid)
        {
            verificationCode.FailedAttempts++;

            int remaining =
                MaxFailedAttempts -
                verificationCode.FailedAttempts;

            if (remaining <= 0)
            {
                verificationCode.IsUsed = true;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (
                false,
                $"Incorrect reactivation code. " +
                $"Remaining attempts: {remaining}."
            );
        }

        // ======================================
        // REACTIVATE CUSTOMER ACCOUNT
        // ======================================

        DateTime now = DateTime.UtcNow;

        user!.IsActive = true;

        user.DeactivatedAt = null;

        user.DeactivationReason = null;

        // Generate new Security Stamp
        user.SecurityStamp =
            Guid.NewGuid().ToString("N");

        user.UpdatedAt = now;

        // Invalidate all remaining verification codes
        var verificationCodes =
            await _context.VerificationCodes
                .Where(v =>
                    v.UserId == userId &&
                    !v.IsUsed)
                .ToListAsync();

        foreach (var item in verificationCodes)
        {
            item.IsUsed = true;
        }

        // Invalidate all remaining password reset requests
        var passwordResets =
            await _context.PasswordResets
                .Where(r =>
                    r.UserId == userId &&
                    !r.IsUsed)
                .ToListAsync();

        foreach (var reset in passwordResets)
        {
            reset.IsUsed = true;
        }

        verificationCode.IsUsed = true;

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return (
            true,
            "Your account has been reactivated successfully."
        );
    }

    // ==========================================
    // 5. HASH REACTIVATION OTP
    // ==========================================

    private string HashCode(
        int userId,
        DateTime deactivatedAt,
        string code)
    {
        string value =
            $"reactivation:{userId}:{deactivatedAt.Ticks}:{code}";

        using var hmac =
            new HMACSHA256(_secretKey);

        byte[] hash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToBase64String(hash);
    }
}