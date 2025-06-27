using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyComfort.Shared.DTOs.Seller
{
    public class CombinedOrdersDto
    {
        public List<CustomerOrderDto> CustomerOrders { get; set; } = new();
        public List<SellerDistributorOrderDto> DistributorOrders { get; set; } = new();
        public int TotalCustomerOrders { get; set; }
        public int TotalDistributorOrders { get; set; }
    }
}
