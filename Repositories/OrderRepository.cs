using Microsoft.EntityFrameworkCore;
using SaigonBite.Data;
using SaigonBite.Models;

namespace SaigonBite.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Include(o => o.Hub)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int voucherId)
        {
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherId == voucherId);
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Hub?> GetHubByIdAsync(int hubId)
        {
            return await _context.Hubs
                .FirstOrDefaultAsync(h => h.HubId == hubId);
        }
    }
}