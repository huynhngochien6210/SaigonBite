namespace SaigonBite.Services
{
    public class PricingResult
    {
        public decimal Subtotal { get; set; }

        public decimal QuietKitchenDiscount { get; set; }

        public decimal VoucherDiscount { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal RushHourSurcharge { get; set; }

        public decimal HeavyRainSurcharge { get; set; }

        public decimal RawTotal { get; set; }

        public decimal FinalTotal { get; set; }
    }
}