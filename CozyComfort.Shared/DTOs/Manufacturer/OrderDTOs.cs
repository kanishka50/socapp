using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CozyComfort.Shared.Enums;

namespace CozyComfort.Shared.DTOs.Manufacturer
{
    public class ManufacturerOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public string DistributorOrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public List<ManufacturerOrderItemDto> Items { get; set; } = new();
    }

    public class ManufacturerOrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateManufacturerOrderFromDistributorDto
    {
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public string DistributorOrderNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty; // "Accepted" or "Cancelled"
        public string? Notes { get; set; }
    }

    public class ManufacturerInventoryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock { get; set; }
        public int MinStockLevel { get; set; }
        public decimal ManufacturingCost { get; set; }
        public bool NeedsProduction => AvailableStock < MinStockLevel;
    }
}
