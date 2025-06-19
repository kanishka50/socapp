using CozyComfort.Shared.Models;

namespace CozyComfort.Distributor.API.Models.Entities
{
    public class DistributorOrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public virtual DistributorOrder Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual DistributorProduct Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public int? QuantityReceived { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}