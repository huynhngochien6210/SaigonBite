using SaigonBite.Models;
using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public interface IAuthenticationService
{
    Task<(bool Success,
          string Message,
          UserAccount? User)>
        LoginCustomerAsync(LoginViewModel model);

    Task<(bool Success,
          string Message,
          UserAccount? User)>
        LoginAdminAsync(AdminLoginViewModel model);
}
