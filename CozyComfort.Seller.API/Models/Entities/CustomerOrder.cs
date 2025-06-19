using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Seller.API.Models.Entities
{
    public class CustomerOrder : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation properties
        public virtual ICollection<CustomerOrderItem> OrderItems { get; set; } = new List<CustomerOrderItem>();
    }
}