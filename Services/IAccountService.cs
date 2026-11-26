
using SaigonBite.ViewModels;

namespace SaigonBite.Services;

public interface IAccountService
{
    Task<(bool Success, string Message, int UserId)>
        RegisterLocalAsync(RegisterLocalViewModel model);

    Task<(bool Success, string Message, int UserId)>
        RegisterForeignAsync(RegisterForeignViewModel model);
}
