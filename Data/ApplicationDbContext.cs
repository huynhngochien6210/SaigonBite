using Microsoft.EntityFrameworkCore;
using SaigonBite.Models;

namespace SaigonBite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Hub> Hubs { get; set; }

        public DbSet<Voucher> Vouchers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Order
            modelBuilder.Entity<Order>()
                .Property(o => o.FinalTotal)
                .HasPrecision(18, 2);


            // OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);


            // Voucher
            modelBuilder.Entity<Voucher>()
                .Property(v => v.DiscountValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Voucher>()
                .Property(v => v.MaxDiscount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Voucher>()
                .Property(v => v.MinSubtotal)
                .HasPrecision(18, 2);


            // Customer 1 --- 0..* Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // Hub 1 --- 0..* Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Hub)
                .WithMany(h => h.Orders)
                .HasForeignKey(o => o.HubId)
                .OnDelete(DeleteBehavior.Restrict);


            // Order 1 --- 1..* OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            // Order 0..1 --- Voucher
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}