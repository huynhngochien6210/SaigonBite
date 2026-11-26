using System.ComponentModel.DataAnnotations;

namespace SaigonBite.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}