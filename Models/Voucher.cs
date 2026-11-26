using System.ComponentModel.DataAnnotations;

namespace SaigonBite.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public VoucherType Type { get; set; }

        public decimal DiscountValue { get; set; }

        public decimal? MaxDiscount { get; set; }

        public decimal MinSubtotal { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        public ICollection<Order> Orders { get; set; }
            = new List<Order>();


        public bool IsValid(decimal subtotal)
        {
            var now = DateTime.Now;

            return now >= StartDate
                && now <= EndDate
                && subtotal >= MinSubtotal;
        }

        public decimal CalculateDiscount(decimal subtotal)
        {
            if (!IsValid(subtotal))
                return 0;

            if (Type == VoucherType.Percentage)
            {
                var discount = subtotal * DiscountValue / 100;

                if (MaxDiscount.HasValue)
                    discount = Math.Min(discount, MaxDiscount.Value);

                return discount;
            }

            return Math.Min(DiscountValue, subtotal);
        }
    }
}