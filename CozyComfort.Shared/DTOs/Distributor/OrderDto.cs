namespace CozyComfort.Shared.DTOs.Distributor
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateManufacturerOrderDto
    {
        public List<OrderItemRequest> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class OrderItemRequest
    {
        public int ManufacturerProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class ProcessSellerOrderDto
    {
        public int SellerId { get; set; }
        public string SellerOrderNumber { get; set; } = string.Empty;
        public List<SellerOrderItem> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class SellerOrderItem
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
        public decimal RequestedPrice { get; set; }
    }

    public class DistributorOrderItem
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}