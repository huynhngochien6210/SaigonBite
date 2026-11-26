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

        // Use Case E - Manage User Accounts
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<LocalCustomer> LocalCustomers { get; set; }
        public DbSet<ForeignTourist> ForeignTourists { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // ==========================================
            // UC-D & UC-E CUSTOMER ACCOUNT INTEGRATION
            // ==========================================

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.UserId)
                    .IsUnique()
                    .HasFilter("[UserId] IS NOT NULL");

                entity.HasOne(c => c.UserAccount)
                    .WithOne()
                    .HasForeignKey<Customer>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

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
                
                
            // ==========================================
            // USER ACCOUNTS
            // ==========================================

            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.ToTable("UserAccounts");

                entity.HasKey(x => x.UserId);

                entity.HasIndex(x => x.Username)
                    .IsUnique()
                    .HasFilter("[Username] IS NOT NULL");

                entity.Property(x => x.Role)
                    .HasConversion<string>()
                    .HasMaxLength(30);

                entity.Property(x => x.AccountType)
                    .HasConversion<string>()
                    .HasMaxLength(30);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_UserAccounts_Role_AccountType",
                    @"(
                        [Role] = 'Customer'
                        AND [AccountType] IS NOT NULL
                        AND [AccountType] IN
                            ('LocalCustomer', 'ForeignTourist')
                    )
                    OR
                    (
                        [Role] IN ('Admin', 'KitchenStaff', 'Shipper')
                        AND [AccountType] IS NULL
                        AND [Username] IS NOT NULL
                    )"
                ));
            });

            // ==========================================
            // LOCAL CUSTOMERS
            // ==========================================

            modelBuilder.Entity<LocalCustomer>(entity =>
            {
                entity.ToTable("LocalCustomers");

                entity.HasKey(x => x.UserId);

                entity.HasIndex(x => x.PhoneNumber)
                    .IsUnique();

                entity.HasOne(x => x.UserAccount)
                    .WithOne(x => x.LocalCustomer)
                    .HasForeignKey<LocalCustomer>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_LocalCustomers_PhoneNumber",
                    @"LEN([PhoneNumber]) = 10
                      AND LEFT([PhoneNumber], 2)
                          IN ('03', '05', '07', '08', '09')
                      AND [PhoneNumber] COLLATE Latin1_General_100_BIN2
                          NOT LIKE '%[^0-9]%'"
                ));
            });

            // ==========================================
            // FOREIGN TOURISTS
            // ==========================================

            modelBuilder.Entity<ForeignTourist>(entity =>
            {
                entity.ToTable("ForeignTourists");

                entity.HasKey(x => x.UserId);

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.HasIndex(x => x.PassportNumber)
                    .IsUnique();

                entity.HasOne(x => x.UserAccount)
                    .WithOne(x => x.ForeignTourist)
                    .HasForeignKey<ForeignTourist>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_ForeignTourists_PassportNumber",
                    @"LEN([PassportNumber]) BETWEEN 6 AND 9
                      AND [PassportNumber] COLLATE Latin1_General_100_BIN2
                          NOT LIKE '%[^A-Za-z0-9]%'"
                ));
            });

            // ==========================================
            // VERIFICATION CODES
            // ==========================================

            modelBuilder.Entity<VerificationCode>(entity =>
            {
                entity.ToTable("VerificationCodes");

                entity.HasKey(x => x.VerificationId);

                entity.HasOne(x => x.UserAccount)
                    .WithMany(x => x.VerificationCodes)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.IsUsed,
                    x.ExpiresAt
                });

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_VerificationCodes_FailedAttempts",
                    "[FailedAttempts] >= 0"
                ));
            });

            // ==========================================
            // PASSWORD RESETS
            // ==========================================

            modelBuilder.Entity<PasswordReset>(entity =>
            {
                entity.ToTable("PasswordResets");

                entity.HasKey(x => x.ResetId);

                entity.HasOne(x => x.UserAccount)
                    .WithMany(x => x.PasswordResets)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.IsUsed,
                    x.ExpiresAt
                });

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique()
                    .HasFilter("[TokenHash] IS NOT NULL");

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_PasswordResets_FailedAttempts",
                    "[FailedAttempts] >= 0"
                ));
            });
        }
    }
}