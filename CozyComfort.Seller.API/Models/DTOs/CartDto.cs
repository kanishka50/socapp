namespace CozyComfort.Seller.API.Models.DTOs
{
    public class CartDto
    {
        public string SessionId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public decimal Tax => SubTotal * 0.10m; // 10% tax
        public decimal ShippingCost => Items.Any() ? 10.00m : 0;
        public decimal Total => SubTotal + Tax + ShippingCost;
        public int ItemCount => Items.Sum(i => i.Quantity);
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}