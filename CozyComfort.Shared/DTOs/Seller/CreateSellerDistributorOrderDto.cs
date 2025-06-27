using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyComfort.Shared.DTOs.Seller
{
    

    // CreateSellerDistributorOrderDto.cs
    public class CreateSellerDistributorOrderDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<CreateSellerDistributorOrderItemDto> Items { get; set; } = new();
    }

    public class CreateSellerDistributorOrderItemDto
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
    }
}
