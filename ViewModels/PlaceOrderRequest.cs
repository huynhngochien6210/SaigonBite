using SaigonBite.Models;

namespace SaigonBite.ViewModels
{
    public class PlaceOrderRequest
    {
        public int CustomerId { get; set; }
        public int HubId { get; set; }
        public decimal DistanceKm { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public int? VoucherId { get; set; }

        public List<OrderItemRequest> Items { get; set; }
            = new List<OrderItemRequest>();

        public decimal QuietKitchenDiscount { get; set; }

        public decimal VoucherDiscount { get; set; }

        public decimal DeliveryFee { get; set; }
        public decimal RushHourSurcharge { get; set; }
        public decimal HeavyRainSurcharge { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class OrderItemRequest
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}