using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Manufacturer.API.Models.Entities
{
    public class ManufacturerOrder : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public string DistributorOrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } // Pending, Accepted, Cancelled
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<ManufacturerOrderItem> OrderItems { get; set; } = new List<ManufacturerOrderItem>();
    }
}