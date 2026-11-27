using SaigonBite.ViewModels;

namespace SaigonBite.Services
{
    public interface IOrderHistoryService
    {
        Task<OrderHistoryViewModel> GetOrderHistoryAsync(
            int userId);
    }
}