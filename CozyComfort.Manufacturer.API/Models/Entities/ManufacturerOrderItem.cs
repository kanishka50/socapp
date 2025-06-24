using CozyComfort.Shared.Models;

namespace CozyComfort.Manufacturer.API.Models.Entities
{
    public class ManufacturerOrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public virtual ManufacturerOrder Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual ManufacturerProduct Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}