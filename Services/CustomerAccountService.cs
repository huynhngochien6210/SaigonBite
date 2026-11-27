
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SaigonBite.Data;
using SaigonBite.Models;
using SaigonBite.Enums;

namespace SaigonBite.Services
{
    public class CustomerAccountService
        : ICustomerAccountService
    {
        private readonly ApplicationDbContext _context;

        public CustomerAccountService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. GET CUSTOMER ID
        // ==========================================

        public async Task<int?> GetCustomerIdAsync(
            int userId)
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c =>
                    c.UserId == userId &&
                    c.UserAccount != null &&
                    c.UserAccount.Role == UserRole.Customer &&
                    c.UserAccount.IsActive &&
                    c.UserAccount.IsVerified)
                .Select(c => (int?)c.CustomerId)
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // 2. GET OR CREATE CUSTOMER
        // ==========================================

        public async Task<Customer> GetOrCreateCustomerAsync(
            int userId)
        {
            var user = await _context.UserAccounts
                .AsNoTracking()
                .Include(u => u.LocalCustomer)
                .Include(u => u.ForeignTourist)
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.Role == UserRole.Customer &&
                    u.IsActive &&
                    u.IsVerified);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Invalid or inactive customer account.");
            }

            // Check existing customer

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId);

            if (existingCustomer != null)
            {
                return existingCustomer;
            }

            string phone;
            string email;

            // Local Customer

            if (user.AccountType == AccountType.LocalCustomer &&
                user.LocalCustomer != null)
            {
                phone = user.LocalCustomer.PhoneNumber;

                email = user.LocalCustomer.Email
                    ?? string.Empty;
            }

            // Foreign Tourist

            else if (
                user.AccountType == AccountType.ForeignTourist &&
                user.ForeignTourist != null)
            {
                phone = user.ForeignTourist.PhoneNumber
                    ?? string.Empty;

                email = user.ForeignTourist.Email;
            }
            else
            {
                throw new InvalidOperationException(
                    "Customer profile is incomplete.");
            }

            // Prevent exceeding current UC-D column length

            if (email.Length > 150)
            {
                throw new InvalidOperationException(
                    "Email exceeds the supported customer email length.");
            }

            // Create new customer

            var customer = new Customer
            {
                UserId = user.UserId,

                Name = user.FullName,

                Phone = phone,

                Email = email
            };

            _context.Customers.Add(customer);

            try
            {
                await _context.SaveChangesAsync();

                return customer;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx &&
                    (sqlEx.Number == 2601 ||
                     sqlEx.Number == 2627))
            {
                // Handle concurrent customer creation

                _context.Entry(customer).State =
                    EntityState.Detached;

                var concurrentCustomer =
                    await _context.Customers
                        .FirstOrDefaultAsync(c =>
                            c.UserId == userId);

                if (concurrentCustomer != null)
                {
                    return concurrentCustomer;
                }

                throw;
            }
        }

    
        // ==========================================
        // 3. CHECK ORDER OWNERSHIP
        // ==========================================

        public async Task<bool> OwnsOrderAsync(
            int userId,
            int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .AnyAsync(o =>
                    o.OrderId == orderId &&
                    o.Customer != null &&
                    o.Customer.UserId == userId &&
                    o.Customer.UserAccount != null &&
                    o.Customer.UserAccount.Role ==
                        UserRole.Customer &&
                    o.Customer.UserAccount.IsActive &&
                    o.Customer.UserAccount.IsVerified);
        }
    }
}