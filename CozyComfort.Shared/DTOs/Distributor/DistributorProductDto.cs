namespace CozyComfort.Shared.DTOs.Distributor
{
    public class DistributorProductDto
    {
        public int Id { get; set; }
        public int ManufacturerProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int AvailableStock { get; set; }
        public int MinStockLevel { get; set; }
        public decimal ProfitMargin => SellingPrice - PurchasePrice;
        public decimal ProfitPercentage => PurchasePrice > 0 ? ((SellingPrice - PurchasePrice) / PurchasePrice) * 100 : 0;
    }

    public class CreateDistributorProductDto
    {
        public int ManufacturerProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int MinStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
    }

    public class UpdateDistributorProductDto
    {
        public decimal SellingPrice { get; set; }
        public int MinStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
    }
}