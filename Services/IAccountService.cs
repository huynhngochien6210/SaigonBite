using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public interface IAccountService
{
    // Register Local Customer
    Task<(bool Success, string Message, int UserId)>
        RegisterLocalAsync(RegisterLocalViewModel model);

    // Register Foreign Tourist
    Task<(bool Success, string Message, int UserId)>
        RegisterForeignAsync(RegisterForeignViewModel model);

    // View Account
    Task<AccountProfileViewModel?>
        GetProfileAsync(int userId);

    // Edit Account
    Task<(bool Success, string Message)>
        UpdateProfileAsync(
            int userId,
            EditAccountViewModel model);

    // Change Password
    Task<(bool Success, string Message)>
        ChangePasswordAsync(
            int userId,
            ChangePasswordViewModel model);

    // Deactivate Account
    Task<(bool Success, string Message)>
        DeactivateAccountAsync(
            int userId,
            DeactivateAccountViewModel model);
}