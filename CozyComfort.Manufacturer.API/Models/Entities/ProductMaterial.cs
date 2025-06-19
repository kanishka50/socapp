using CozyComfort.Shared.Models;

namespace CozyComfort.Manufacturer.API.Models.Entities
{
    public class ProductMaterial : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual ManufacturerProduct Product { get; set; } = null!;

        public string MaterialName { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public string Unit { get; set; } = string.Empty; // meters, kg, etc.
        public decimal CostPerUnit { get; set; }
    }
}