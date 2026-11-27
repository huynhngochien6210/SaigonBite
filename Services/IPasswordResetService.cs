
using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public interface IPasswordResetService
{
    Task<(bool Success, string Message, int? UserId)>
        RequestResetAsync(ForgotPasswordViewModel model);

    Task<(bool Success, string Message)>
        SendCodeAsync(int userId);

    Task<(bool Success, string Message, string? Token)>
        VerifyCodeAsync(int userId, string code);

    Task<(bool Success, string Message)>
        ResetPasswordAsync(
            int userId,
            string token,
            ResetPasswordViewModel model);
}
