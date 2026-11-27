using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonBite.Models
{
    public class PasswordReset
    {
        [Key]
        public int ResetId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string CodeHash { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? TokenHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime? TokenExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public int FailedAttempts { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public UserAccount UserAccount { get; set; } = null!;
    }
}