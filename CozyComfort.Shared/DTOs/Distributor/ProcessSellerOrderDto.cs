using System.Collections.Generic;

namespace CozyComfort.Shared.DTOs.Distributor
{
    public class ProcessSellerOrderDto
    {
        public int SellerId { get; set; }
        public string SellerOrderNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public List<ProcessSellerOrderItemDto> Items { get; set; } = new();
    }

    public class ProcessSellerOrderItemDto
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
    }
}