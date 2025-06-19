using CozyComfort.Shared.Models;

namespace CozyComfort.Distributor.API.Models.Entities
{
    public class DistributorProduct : BaseEntity
    {
        public int ManufacturerProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock => CurrentStock - ReservedStock;
        public int MinStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }

        // Navigation properties
        public virtual ICollection<DistributorInventoryTransaction> InventoryTransactions { get; set; } = new List<DistributorInventoryTransaction>();
        public virtual ICollection<DistributorOrderItem> OrderItems { get; set; } = new List<DistributorOrderItem>();
    }
}