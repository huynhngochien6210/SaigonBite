using System.ComponentModel.DataAnnotations;

namespace SaigonBite.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ItemName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }


        // Order
        public int OrderId { get; set; }

        public Order? Order { get; set; }
    }
}