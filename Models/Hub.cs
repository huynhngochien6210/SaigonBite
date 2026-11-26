using System.ComponentModel.DataAnnotations;

namespace SaigonBite.Models
{
    public class Hub
    {
        public int HubId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int CurrentLoad { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
}