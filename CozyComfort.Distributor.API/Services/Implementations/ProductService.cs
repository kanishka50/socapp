using CozyComfort.Distributor.API.Data;
using CozyComfort.Distributor.API.Models.Entities;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
//using CozyComfort.Distributor.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Manufacturer;
using CozyComfort.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly DistributorDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(DistributorDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<DistributorProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var query = _context.Products.Where(p => p.IsActive);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(p =>
                        p.ProductName.Contains(request.SearchTerm) ||
                        p.SKU.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new DistributorProductDto
                    {
                        Id = p.Id,
                        ManufacturerProductId = p.ManufacturerProductId ?? 0,
                        ProductName = p.ProductName,
                        SKU = p.SKU,
                        PurchasePrice = p.PurchasePrice,
                        SellingPrice = p.SellingPrice,
                        CurrentStock = p.CurrentStock,
                        AvailableStock = p.AvailableStock,
                        MinStockLevel = p.MinStockLevel
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<DistributorProductDto>
                {
                    Items = products,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResult<DistributorProductDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return ApiResponse<PagedResult<DistributorProductDto>>.FailureResult("Error retrieving products");
            }
        }

        public async Task<ApiResponse<DistributorStockCheckResponse>> CheckStockAsync(DistributorStockCheckRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return ApiResponse<DistributorStockCheckResponse>.FailureResult("Product not found");
                }

                var response = new DistributorStockCheckResponse
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    SKU = product.SKU,
                    QuantityRequested = request.QuantityRequested,
                    AvailableStock = product.AvailableStock,
                    IsAvailable = product.AvailableStock >= request.QuantityRequested,
                    NeedsReorder = product.CurrentStock <= product.ReorderPoint,
                    SuggestedReorderQuantity = product.ReorderQuantity
                };

                if (response.IsAvailable)
                {
                    response.Message = "Stock available for immediate fulfillment";
                }
                else
                {
                    response.Message = $"Insufficient stock. Only {product.AvailableStock} units available";
                }

                return ApiResponse<DistributorStockCheckResponse>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock");
                return ApiResponse<DistributorStockCheckResponse>.FailureResult("Error checking stock");
            }
        }

        // Implement other methods with basic logic or NotImplementedException for now
        public async Task<ApiResponse<DistributorProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Where(p => p.Id == id && p.IsActive)
                    .Select(p => new DistributorProductDto
                    {
                        Id = p.Id,
                        ManufacturerProductId = p.ManufacturerProductId,
                        ProductName = p.ProductName,
                        SKU = p.SKU,
                        PurchasePrice = p.PurchasePrice,
                        SellingPrice = p.SellingPrice,
                        CurrentStock = p.CurrentStock,
                        AvailableStock = p.AvailableStock,
                        MinStockLevel = p.MinStockLevel
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return ApiResponse<DistributorProductDto>.FailureResult("Product not found");
                }

                return ApiResponse<DistributorProductDto>.SuccessResult(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return ApiResponse<DistributorProductDto>.FailureResult("Error retrieving product");
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> AddProductFromManufacturerAsync(CreateDistributorProductDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<DistributorProductDto>> UpdateProductAsync(int id, UpdateDistributorProductDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> UpdateStockAsync(int productId, UpdateDistributorStockDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<DistributorProduct>> CreateProductFromManufacturerAsync(ProductDto manufacturerProduct, decimal purchasePrice = 0)
        {
            try
            {
                // Check if product already exists
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ManufacturerProductId == manufacturerProduct.Id);

                if (existingProduct != null)
                {
                    return ApiResponse<DistributorProduct>.SuccessResult(existingProduct);
                }

                // Create new distributor product with 0 stock
                var distributorProduct = new DistributorProduct
                {
                    ManufacturerProductId = manufacturerProduct.Id,
                    ProductName = manufacturerProduct.Name,
                    SKU = manufacturerProduct.SKU,
                    PurchasePrice = purchasePrice > 0 ? purchasePrice : manufacturerProduct.Price * 0.7m, // 30% margin by default
                    SellingPrice = manufacturerProduct.Price,
                    CurrentStock = 0, // Start with 0 stock
                    ReservedStock = 0,
                    MinStockLevel = 5,
                    ReorderPoint = 10,
                    ReorderQuantity = 20,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Products.Add(distributorProduct);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created distributor product {distributorProduct.SKU} from manufacturer product {manufacturerProduct.Id}");
                return ApiResponse<DistributorProduct>.SuccessResult(distributorProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor product from manufacturer product {ProductId}", manufacturerProduct.Id);
                return ApiResponse<DistributorProduct>.FailureResult("Error creating product");
            }
        }
    }
}