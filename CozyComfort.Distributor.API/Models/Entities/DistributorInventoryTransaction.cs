using CozyComfort.Shared.Models;

namespace CozyComfort.Distributor.API.Models.Entities
{
    public class DistributorInventoryTransaction : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual DistributorProduct Product { get; set; } = null!;

        public string TransactionType { get; set; } = string.Empty; // IN, OUT, RESERVED, CANCELLED
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty; // Order Number
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal? UnitCost { get; set; }
    }
}