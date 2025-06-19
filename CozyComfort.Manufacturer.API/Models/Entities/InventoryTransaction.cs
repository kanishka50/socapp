using CozyComfort.Shared.Models;
using System;

namespace CozyComfort.Manufacturer.API.Models.Entities
{
    public class InventoryTransaction : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual ManufacturerProduct Product { get; set; } = null!;

        public string TransactionType { get; set; } = string.Empty; // IN, OUT, RESERVED, CANCELLED
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty; // Order ID or Production ID
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }
}