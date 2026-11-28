using Microsoft.EntityFrameworkCore;
using SaigonBite.Models;
using SaigonBite.Enums;

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

        // UC-E
        public DbSet<UserAccount> UserAccounts { get; set; }

        public DbSet<LocalCustomer> LocalCustomers { get; set; }

        public DbSet<ForeignTourist> ForeignTourists { get; set; }

        public DbSet<VerificationCode> VerificationCodes { get; set; }

        public DbSet<PasswordReset> PasswordResets { get; set; }
        //uca
        
        public DbSet<Brand> Brands { get; set; }

        public DbSet<Food> Foods { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // UC-D & UC-E INTEGRATION
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

                // Security Stamp
                entity.Property(x => x.SecurityStamp)
                    .HasMaxLength(64)
                    .IsRequired();

                // Account Reactivation
                entity.Property(x => x.DeactivationReason)
                    .HasConversion<string>()
                    .HasMaxLength(30);

                entity.Property(x => x.DeactivatedAt)
                    .IsRequired(false);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_UserAccounts_DeactivationStatus",
                    @"(
            [IsActive] = 1
            AND [DeactivationReason] IS NULL
            AND [DeactivatedAt] IS NULL
        )
        OR
        (
            [IsActive] = 0
            AND
            (
                (
                    [DeactivationReason] IS NULL
                    AND [DeactivatedAt] IS NULL
                )
                OR
                (
                    [DeactivationReason] IS NOT NULL
                    AND [DeactivationReason] IN
                        ('SelfDeactivated', 'AdminSuspended')
                    AND [DeactivatedAt] IS NOT NULL
                )
            )
        )"
                ));

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

                entity.Property(x => x.Purpose)
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .HasDefaultValue(
                        VerificationPurpose.Registration)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.IsUsed,
                    x.ExpiresAt
                });

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.Purpose,
                    x.CreatedAt
                });

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_VerificationCodes_FailedAttempts",
                    "[FailedAttempts] >= 0"
                ));

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_VerificationCodes_Purpose",
                    "[Purpose] IN ('Registration', 'Reactivation')"
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
            // ==========================================
            // UC-A: BRAND & FOOD MANAGEMENT
            // ==========================================

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("Brands");

                entity.HasKey(x => x.BrandId);

                entity.Property(x => x.BrandName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.Logo)
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Food>(entity =>
            {
                entity.ToTable("Foods");

                entity.HasKey(x => x.FoodId);

                entity.Property(x => x.FoodName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Category)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.ImageUrl)
                    .HasMaxLength(255);

                entity.HasOne(x => x.Brand)
                    .WithMany(x => x.Foods)
                    .HasForeignKey(x => x.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}