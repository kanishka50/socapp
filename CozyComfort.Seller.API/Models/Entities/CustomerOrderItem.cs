using CozyComfort.Shared.Models;

namespace CozyComfort.Seller.API.Models.Entities
{
    public class CustomerOrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public virtual CustomerOrder Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual SellerProduct Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}