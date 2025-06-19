using CozyComfort.Shared.Models;
using System.Collections.Generic;

namespace CozyComfort.Manufacturer.API.Models.Entities
{
    public class ManufacturerProduct : Product
    {
        public int CurrentStock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock => CurrentStock - ReservedStock;
        public int ProductionCapacityPerDay { get; set; }
        public decimal ManufacturingCost { get; set; }
        public int LeadTimeDays { get; set; }

        // Navigation properties
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    }
}