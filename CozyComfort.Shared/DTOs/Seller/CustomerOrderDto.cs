namespace CozyComfort.Shared.DTOs.Seller
{
    public class CustomerOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
    }

    public class CreateCustomerOrderDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
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

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    // For inter-service communication
    public class DistributorOrderItem
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
        public decimal RequestedPrice { get; set; }
    }

    public class CreateDistributorOrderRequest
    {
        public List<DistributorOrderItem> Items { get; set; } = new();
        public string OrderReference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}