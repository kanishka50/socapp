namespace CozyComfort.Shared.DTOs.Seller
{
    public class UpdateStockFromOrderDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public List<StockUpdateItem> Items { get; set; } = new();
    }

    public class StockUpdateItem
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
    }
}