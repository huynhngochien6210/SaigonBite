using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonBite.Models
{
    public class LocalCustomer
    {
        [Key]
        [ForeignKey(nameof(UserAccount))]
        public int UserId { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^0[35789][0-9]{8}$")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(254)]
        [EmailAddress]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public UserAccount UserAccount { get; set; } = null!;
    }
}