namespace SaigonBite.Services;

public interface IAccountReactivationService
{
    Task<(bool Success, string Message)>
        SendCodeAsync(
            int userId,
            DateTime deactivatedAt);

    Task<(bool Success, string Message)>
        VerifyCodeAsync(
            int userId,
            DateTime deactivatedAt,
            string code);
}