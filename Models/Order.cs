using System.ComponentModel.DataAnnotations;

namespace SaigonBite.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime OrderTime { get; set; }

        public OrderStatus Status { get; set; }

        [Required]
        [MaxLength(255)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public decimal FinalTotal { get; set; }


        // Customer
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }


        // Hub
        public int HubId { get; set; }

        public Hub? Hub { get; set; }


        // Voucher - optional
        public int? VoucherId { get; set; }

        public Voucher? Voucher { get; set; }


        // Order items
        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}