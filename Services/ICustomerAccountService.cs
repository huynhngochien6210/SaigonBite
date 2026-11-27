using SaigonBite.Models;

namespace SaigonBite.Services
{
    public interface ICustomerAccountService
    {
        Task<int?> GetCustomerIdAsync(int userId);

        Task<Customer> GetOrCreateCustomerAsync(int userId);

        Task<bool> OwnsOrderAsync(int userId, int orderId);
    }
}