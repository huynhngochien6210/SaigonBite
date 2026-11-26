
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Data;
using SaigonBite.Enums;
using SaigonBite.Models;

namespace SaigonBite.Services;

public class VerificationService : IVerificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly byte[] _secretKey;

    private const int ExpirationMinutes = 5;
    private const int MaxFailedAttempts = 5;
    private const int ResendSeconds = 60;

    public VerificationService(
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

    // Generate and send OTP
    public async Task<(bool Success, string Message)>
        SendCodeAsync(int userId)
    {
        // Only allow simulated delivery in Development
        if (!_environment.IsDevelopment())
        {
            return (false,
                "OTP delivery is not configured.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == userId);

        // Validate account
        if (user == null ||
            !user.IsActive ||
            user.Role != UserRole.Customer)
        {
            return (false, "Invalid account.");
        }

        if (user.IsVerified)
        {
            return (false,
                "This account is already verified.");
        }

        DateTime now = DateTime.UtcNow;

        // Check latest OTP
        var latestCode = await _context.VerificationCodes
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ThenByDescending(v => v.VerificationId)
            .FirstOrDefaultAsync();

        // Prevent frequent resend
        if (latestCode != null &&
            now < latestCode.CreatedAt.AddSeconds(ResendSeconds))
        {
            return (false,
                "Please wait 60 seconds before requesting another code.");
        }

        // Limit OTP requests to 5 per hour
        int recentRequests = await _context.VerificationCodes
            .CountAsync(v =>
                v.UserId == userId &&
                v.CreatedAt >= now.AddHours(-1));

        if (recentRequests >= 5)
        {
            return (false,
                "Too many verification requests. Please try again later.");
        }

        // Invalidate previous OTPs
        var oldCodes = await _context.VerificationCodes
            .Where(v =>
                v.UserId == userId &&
                !v.IsUsed)
            .ToListAsync();

        foreach (var oldCode in oldCodes)
        {
            oldCode.IsUsed = true;
        }

        // Generate secure 6-digit OTP
        string otp = RandomNumberGenerator
            .GetInt32(0, 1000000)
            .ToString("D6");

        // Create verification record
        var verificationCode = new VerificationCode
        {
            UserId = userId,
            CodeHash = HashCode(userId, otp),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(ExpirationMinutes),
            IsUsed = false,
            FailedAttempts = 0
        };

        _context.VerificationCodes.Add(verificationCode);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        // Simulated OTP delivery - Development only
        Console.WriteLine();
        Console.WriteLine("==============================");
        Console.WriteLine("SAIGONBITE OTP SIMULATION");
        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"OTP Code: {otp}");
        Console.WriteLine("Expires in: 5 minutes");
        Console.WriteLine("==============================");
        Console.WriteLine();

        return (true,
            "Verification code generated successfully.");
    }

    // Verify OTP
    public async Task<(bool Success, string Message)>
        VerifyCodeAsync(int userId, string code)
    {
        // Validate OTP format
        if (string.IsNullOrEmpty(code) ||
            !Regex.IsMatch(code, @"\A[0-9]{6}\z"))
        {
            return (false,
                "Please enter a valid 6-digit code.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null ||
            !user.IsActive ||
            user.Role != UserRole.Customer)
        {
            return (false, "Invalid account.");
        }

        if (user.IsVerified)
        {
            return (false,
                "This account is already verified.");
        }

        // Find latest verification code
        var verificationCode =
            await _context.VerificationCodes
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ThenByDescending(v => v.VerificationId)
                .FirstOrDefaultAsync();

        if (verificationCode == null)
        {
            return (false,
                "Verification code was not found.");
        }

        if (verificationCode.FailedAttempts >= MaxFailedAttempts)
        {
            return (false,
                "Too many failed attempts. Request a new code.");
        }

        if (verificationCode.IsUsed)
        {
            return (false,
                "Verification code is no longer valid.");
        }

        // Check expiration
        if (verificationCode.ExpiresAt <= DateTime.UtcNow)
        {
            return (false,
                "Verification code has expired.");
        }

        // Compare OTP hashes
        string inputHash = HashCode(userId, code);

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

            return (false,
                $"Incorrect verification code. " +
                $"Remaining attempts: {remaining}.");
        }

        // Correct OTP
        verificationCode.IsUsed = true;

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true,
            "Account verified successfully.");
    }

    // Hash OTP with HMAC-SHA256
    private string HashCode(int userId, string code)
    {
        string value =
            $"registration:{userId}:{code}";

        using var hmac =
            new HMACSHA256(_secretKey);

        byte[] hash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToBase64String(hash);
    }
}
