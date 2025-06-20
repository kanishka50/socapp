namespace CozyComfort.Shared.DTOs.Manufacturer
{
    public class StockCheckRequest
    {
        public int ProductId { get; set; }
        public int QuantityRequested { get; set; }
    }

    public class StockCheckResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? EstimatedProductionDays { get; set; }
        public DateTime? EstimatedAvailabilityDate { get; set; }
    }

    public class UpdateStockDto
    {
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}