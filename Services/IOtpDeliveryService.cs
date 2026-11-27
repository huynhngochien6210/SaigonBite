namespace SaigonBite.Services;

public interface IOtpDeliveryService
{
    bool IsAvailable { get; }

    Task SendAsync(
        int userId,
        string destination,
        string purpose,
        string code,
        CancellationToken cancellationToken = default
    );
}