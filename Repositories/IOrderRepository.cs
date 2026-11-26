using SaigonBite.Models;

namespace SaigonBite.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int orderId);

        Task<List<Order>> GetByCustomerIdAsync(int customerId);

        Task<Voucher?> GetVoucherByIdAsync(int voucherId);

        Task<Hub?> GetHubByIdAsync(int hubId);

        Task AddAsync(Order order);

        Task UpdateAsync(Order order);

        Task SaveChangesAsync();
    }
}