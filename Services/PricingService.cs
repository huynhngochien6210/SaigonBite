namespace SaigonBite.Services
{
    public class PricingService
    {
        public PricingResult Calculate(
            decimal subtotal,
            decimal quietKitchenDiscount,
            decimal voucherDiscount,
            decimal deliveryFee,
            decimal rushHourSurcharge,
            decimal heavyRainSurcharge)
        {
            // Delivery fee được miễn nếu subtotal >= 300,000
            if (subtotal >= 300000)
            {
                deliveryFee = 0;
            }

            // F = S - Q - V
            decimal discountedSubtotal =
                subtotal - quietKitchenDiscount - voucherDiscount;

            // Raw Total = F + D + R + R2
            decimal rawTotal =
                discountedSubtotal
                + deliveryFee
                + rushHourSurcharge
                + heavyRainSurcharge;

            // Làm tròn đến 1,000 VND
            decimal finalTotal =
                Math.Round(
                    rawTotal / 1000,
                    MidpointRounding.AwayFromZero
                ) * 1000;

            return new PricingResult
            {
                Subtotal = subtotal,
                QuietKitchenDiscount = quietKitchenDiscount,
                VoucherDiscount = voucherDiscount,
                DeliveryFee = deliveryFee,
                RushHourSurcharge = rushHourSurcharge,
                HeavyRainSurcharge = heavyRainSurcharge,
                RawTotal = rawTotal,
                FinalTotal = finalTotal
            };
        }
    }
}