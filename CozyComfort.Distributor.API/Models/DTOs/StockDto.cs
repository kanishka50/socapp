namespace CozyComfort.Distributor.API.Models.DTOs
{
    public class DistributorStockCheckRequest
    {
        public int ProductId { get; set; }
        public int QuantityRequested { get; set; }
    }

    public class DistributorStockCheckResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool NeedsReorder { get; set; }
        public int? SuggestedReorderQuantity { get; set; }
    }

    public class UpdateDistributorStockDto
    {
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal? UnitCost { get; set; }
    }
}