using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Seller.API.Models.Entities
{
    public class SellerDistributorOrder : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int DistributorId { get; set; }
        public string DistributorOrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<SellerDistributorOrderItem> OrderItems { get; set; } = new List<SellerDistributorOrderItem>();
    }
}