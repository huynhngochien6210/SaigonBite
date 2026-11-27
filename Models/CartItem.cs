namespace SaigonBite.Models
{
    public class CartItem
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal Total =>
            UnitPrice * Quantity;
    }
}
