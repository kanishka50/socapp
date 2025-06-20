using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CozyComfort.Manufacturer.API.Data;
using CozyComfort.Shared.DTOs.Manufacturer;
//using CozyComfort.Manufacturer.API.Models.DTOs;
using CozyComfort.Manufacturer.API.Models.Entities;
using CozyComfort.Manufacturer.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Manufacturer.API.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ManufacturerDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ManufacturerDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var query = _context.Products.Where(p => p.IsActive);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(p =>
                        p.Name.Contains(request.SearchTerm) ||
                        p.SKU.Contains(request.SearchTerm) ||
                        p.Category.Contains(request.SearchTerm));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => request.IsDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "stock" => request.IsDescending ? query.OrderByDescending(p => p.CurrentStock) : query.OrderBy(p => p.CurrentStock),
                    _ => query.OrderBy(p => p.Id)
                };

                // Apply pagination
                var products = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Material = p.Material,
                        Size = p.Size,
                        Price = p.Price,
                        SKU = p.SKU,
                        Category = p.Category,
                        CurrentStock = p.CurrentStock,
                        AvailableStock = p.AvailableStock,
                        MinStockLevel = p.MinStockLevel,
                        ImageUrl = p.ImageUrl,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<ProductDto>
                {
                    Items = products,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResult<ProductDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return ApiResponse<PagedResult<ProductDto>>.FailureResult("Error retrieving products");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Where(p => p.Id == id && p.IsActive)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Material = p.Material,
                        Size = p.Size,
                        Price = p.Price,
                        SKU = p.SKU,
                        Category = p.Category,
                        CurrentStock = p.CurrentStock,
                        AvailableStock = p.AvailableStock,
                        MinStockLevel = p.MinStockLevel,
                        ImageUrl = p.ImageUrl,
                        IsActive = p.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return ApiResponse<ProductDto>.FailureResult("Product not found");
                }

                return ApiResponse<ProductDto>.SuccessResult(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return ApiResponse<ProductDto>.FailureResult("Error retrieving product");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                // Check if SKU already exists
                var existingProduct = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
                if (existingProduct)
                {
                    return ApiResponse<ProductDto>.FailureResult("Product with this SKU already exists");
                }

                var product = new ManufacturerProduct
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Material = dto.Material,
                    Size = dto.Size,
                    Price = dto.Price,
                    SKU = dto.SKU,
                    Category = dto.Category,
                    MinStockLevel = dto.MinStockLevel,
                    CurrentStock = dto.InitialStock,
                    ProductionCapacityPerDay = dto.ProductionCapacityPerDay,
                    ManufacturingCost = dto.ManufacturingCost,
                    LeadTimeDays = dto.LeadTimeDays,
                    ImageUrl = dto.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Create initial inventory transaction
                var transaction = new InventoryTransaction
                {
                    ProductId = product.Id,
                    TransactionType = "IN",
                    Quantity = dto.InitialStock,
                    Reference = "INITIAL",
                    Notes = "Initial stock entry",
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Material = product.Material,
                    Size = product.Size,
                    Price = product.Price,
                    SKU = product.SKU,
                    Category = product.Category,
                    CurrentStock = product.CurrentStock,
                    AvailableStock = product.AvailableStock,
                    MinStockLevel = product.MinStockLevel,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive
                };

                return ApiResponse<ProductDto>.SuccessResult(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ApiResponse<ProductDto>.FailureResult("Error creating product");
            }
        }

        public async Task<ApiResponse<StockCheckResponse>> CheckStockAsync(StockCheckRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return ApiResponse<StockCheckResponse>.FailureResult("Product not found");
                }

                var response = new StockCheckResponse
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    QuantityRequested = request.QuantityRequested,
                    AvailableStock = product.AvailableStock,
                    IsAvailable = product.AvailableStock >= request.QuantityRequested
                };

                if (response.IsAvailable)
                {
                    response.Message = "Stock available for immediate fulfillment";
                }
                else
                {
                    var shortfall = request.QuantityRequested - product.AvailableStock;
                    var daysNeeded = (int)Math.Ceiling((double)shortfall / product.ProductionCapacityPerDay);

                    response.EstimatedProductionDays = daysNeeded + product.LeadTimeDays;
                    response.EstimatedAvailabilityDate = DateTime.UtcNow.AddDays(response.EstimatedProductionDays.Value);
                    response.Message = $"Insufficient stock. Can produce required quantity in {response.EstimatedProductionDays} days";
                }

                return ApiResponse<StockCheckResponse>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock for product {ProductId}", request.ProductId);
                return ApiResponse<StockCheckResponse>.FailureResult("Error checking stock");
            }
        }

        // Implement remaining methods (UpdateProductAsync, DeleteProductAsync, UpdateStockAsync)
        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            // Implementation here
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            // Implementation here
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> UpdateStockAsync(int productId, UpdateStockDto dto)
        {
            // Implementation here
            throw new NotImplementedException();
        }
    }
}