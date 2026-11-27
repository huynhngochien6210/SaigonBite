namespace SaigonBite.Services;

public class ConsoleOtpDeliveryService
    : IOtpDeliveryService
{
    private readonly IWebHostEnvironment _environment;

    public ConsoleOtpDeliveryService(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool IsAvailable =>
        _environment.IsDevelopment();

    public Task SendAsync(
        int userId,
        string destination,
        string purpose,
        string code,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Console OTP is only available in Development.");
        }

        Console.WriteLine();
        Console.WriteLine("==============================");
        Console.WriteLine("SAIGONBITE - SIMULATED OTP");
        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"Purpose: {purpose}");
        Console.WriteLine($"OTP Code: {code}");
        Console.WriteLine("Expires in: 5 minutes");
        Console.WriteLine("==============================");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}