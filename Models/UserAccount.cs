using System.ComponentModel.DataAnnotations;
using SaigonBite.Enums;

namespace SaigonBite.Models
{
    public class UserAccount
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public AccountType? AccountType { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
	[Required]
[MaxLength(64)]
public string SecurityStamp { get; set; }
    = Guid.NewGuid().ToString("N");

        // Account deactivation information
        public DateTime? DeactivatedAt { get; set; }

        public DeactivationReason? DeactivationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // One-to-One Relationships
        public LocalCustomer? LocalCustomer { get; set; }

        public ForeignTourist? ForeignTourist { get; set; }

        // One-to-Many Relationships
        public ICollection<VerificationCode> VerificationCodes { get; set; }
            = new List<VerificationCode>();

        public ICollection<PasswordReset> PasswordResets { get; set; }
            = new List<PasswordReset>();
    }
}