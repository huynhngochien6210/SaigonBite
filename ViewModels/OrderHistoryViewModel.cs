using SaigonBite.Models;

namespace SaigonBite.ViewModels
{
    public class OrderHistoryViewModel
    {
        public List<OrderHistoryItemViewModel> Orders { get; set; }
            = new();

        public int TotalOrders => Orders.Count;
    }

    public class OrderHistoryItemViewModel
    {
        public int OrderId { get; set; }

        public string OrderCode => $"SB{OrderId:D3}";

        public DateTime OrderDate { get; set; }

        public decimal Total { get; set; }

        public OrderStatus Status { get; set; }

        public string DeliveryAddress { get; set; }
            = string.Empty;

        public List<OrderHistoryProductViewModel> Items { get; set; }
            = new();
    }

    public class OrderHistoryProductViewModel
    {
        public string ItemName { get; set; }
            = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }
}