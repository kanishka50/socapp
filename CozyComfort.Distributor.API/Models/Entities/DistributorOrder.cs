using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Distributor.API.Models.Entities
{
    public class DistributorOrder : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public OrderType OrderType { get; set; } // FROM_MANUFACTURER or FROM_SELLER
        public int? ManufacturerId { get; set; }
        public int? SellerId { get; set; }

        // Add these two properties
        public int? CustomerId { get; set; }  // Generic customer ID (can be Seller or Manufacturer)
        public string? CustomerOrderNumber { get; set; } // To store the original order number from customer

        public string CustomerName { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<DistributorOrderItem> OrderItems { get; set; } = new List<DistributorOrderItem>();
    }

    public enum OrderType
    {
        FromManufacturer = 1,
        FromSeller = 2
    }
}