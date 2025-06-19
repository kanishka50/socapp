namespace CozyComfort.Seller.API.Models.DTOs
{
    public class DistributorOrderItem
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
        public decimal RequestedPrice { get; set; }
    }

    public class CreateDistributorOrderRequest
    {
        public List<DistributorOrderItem> Items { get; set; } = new();
        public string OrderReference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}