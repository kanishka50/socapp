namespace CozyComfort.Shared.DTOs.Seller
{
    public class SellerProductDto
    {
        public int Id { get; set; }
        public int DistributorProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal RetailPrice { get; set; }
        public decimal Price { get; set; } // For backward compatibility
        public int CurrentStock { get; set; }
        public bool IsAvailable { get; set; }
        public bool InStock { get; set; } // For backward compatibility
        public string? ImageUrl { get; set; } = string.Empty;
    }
}