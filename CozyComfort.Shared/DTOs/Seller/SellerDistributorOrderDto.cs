namespace CozyComfort.Shared.DTOs.Seller
{
    public class SellerDistributorOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string DistributorOrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SellerDistributorOrderItemDto> Items { get; set; } = new();
    }

    public class SellerDistributorOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateSellerDistributorOrderDto
    {
        public List<DistributorOrderItemRequest> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class DistributorOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateSellerStockBulkDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public List<SellerStockUpdateItem> Items { get; set; } = new();
    }

    public class SellerStockUpdateItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty;
    }
}