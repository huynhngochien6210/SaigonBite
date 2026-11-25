using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonBite.Models
{
    public class ForeignTourist
    {
        [Key]
        [ForeignKey(nameof(UserAccount))]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(9, MinimumLength = 6)]
        [RegularExpression(@"^[A-Za-z0-9]{6,9}$")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nationality { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public UserAccount UserAccount { get; set; } = null!;
    }
}