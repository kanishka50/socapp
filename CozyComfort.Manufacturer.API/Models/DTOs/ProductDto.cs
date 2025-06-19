using System;

namespace CozyComfort.Manufacturer.API.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int AvailableStock { get; set; }
        public int MinStockLevel { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int MinStockLevel { get; set; }
        public int InitialStock { get; set; }
        public int ProductionCapacityPerDay { get; set; }
        public decimal ManufacturingCost { get; set; }
        public int LeadTimeDays { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int MinStockLevel { get; set; }
        public int ProductionCapacityPerDay { get; set; }
        public decimal ManufacturingCost { get; set; }
        public int LeadTimeDays { get; set; }
        public string? ImageUrl { get; set; }
    }
}