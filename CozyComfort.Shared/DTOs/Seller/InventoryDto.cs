namespace CozyComfort.Shared.DTOs.Seller
{
    // DTOs for Inventory operations
    public class SellerInventoryDto : SellerProductDto
    {
        public int DistributorProductId { get; set; }
        public int DisplayStock { get; set; }
        public decimal PurchasePrice { get; set; }

        public decimal SellingPrice { get; set; }
        public bool NeedsReorder { get; set; }
    }

    public class CheckStockRequestDto
    {
        public int ProductId { get; set; }
        public int QuantityNeeded { get; set; }
    }

    public class CheckStockResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentSellerStock { get; set; }
        public int QuantityNeeded { get; set; }
        public bool DistributorHasStock { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateDistributorOrderDto
    {
        public List<DistributorOrderItemRequest> Items { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    //public class DistributorOrderItemRequest
    //{
    //    public int ProductId { get; set; }
    //    public int Quantity { get; set; }
    //}

    public class UpdateStockDto
    {
        public int NewStock { get; set; }
    }
}