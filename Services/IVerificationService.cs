
namespace SaigonBite.Services;

public interface IVerificationService
{
    Task<(bool Success, string Message)>
        SendCodeAsync(int userId);

    Task<(bool Success, string Message)>
        VerifyCodeAsync(int userId, string code);
}
