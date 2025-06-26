using CozyComfort.Shared.Models;

namespace CozyComfort.Seller.API.Models.Entities
{
    public class SellerInventoryTransaction : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual SellerProduct Product { get; set; } = null!;

        public string TransactionType { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty; // Order Number or Reference
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal? UnitCost { get; set; }
    }
}