using CozyComfort.Shared.Models;

namespace CozyComfort.Seller.API.Models.Entities
{
    public class SellerProduct : BaseEntity
    {
        public int DistributorProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int DisplayStock { get; set; } // What customers see
        public bool IsAvailable { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<CustomerOrderItem> OrderItems { get; set; } = new List<CustomerOrderItem>();
    }
}