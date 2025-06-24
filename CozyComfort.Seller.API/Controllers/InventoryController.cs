using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;



namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SellerOnly")]
    public class InventoryController : ControllerBase
    {
        private readonly SellerDbContext _context;
        private readonly IDistributorApiService _distributorApiService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            SellerDbContext context,
            IDistributorApiService distributorApiService,
            ILogger<InventoryController> logger)
        {
            _context = context;
            _distributorApiService = distributorApiService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetInventory([FromQuery] PagedRequest request)
        {
            try
            {
                var query = _context.SellerProducts.Where(p => p.IsActive);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(p =>
                        p.ProductName.Contains(request.SearchTerm) ||
                        p.SKU.Contains(request.SearchTerm) ||
                        p.Category.Contains(request.SearchTerm));
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.IsDescending ? query.OrderByDescending(p => p.ProductName) : query.OrderBy(p => p.ProductName),
                    "stock" => request.IsDescending ? query.OrderByDescending(p => p.CurrentStock) : query.OrderBy(p => p.CurrentStock),
                    "category" => request.IsDescending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
                    _ => query.OrderBy(p => p.Id)
                };

                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new SellerInventoryDto
                    {
                        Id = p.Id,
                        DistributorProductId = p.DistributorProductId,
                        ProductName = p.ProductName,
                        SKU = p.SKU,
                        Category = p.Category,
                        CurrentStock = p.CurrentStock,
                        DisplayStock = p.DisplayStock,
                        PurchasePrice = p.PurchasePrice,
                        SellingPrice = p.SellingPrice,
                        IsAvailable = p.IsAvailable,
                        NeedsReorder = p.CurrentStock <= 10, // Simple reorder logic
                        ImageUrl = p.ImageUrl
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<SellerInventoryDto>
                {
                    Items = products,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Ok(ApiResponse<PagedResult<SellerInventoryDto>>.SuccessResult(pagedResult));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory");
                return BadRequest(ApiResponse<PagedResult<SellerInventoryDto>>.FailureResult("Error retrieving inventory"));
            }
        }

        [HttpPost("check-distributor-stock")]
        public async Task<IActionResult> CheckDistributorStock([FromBody] CheckStockRequestDto request)
        {
            try
            {
                var product = await _context.SellerProducts.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound(ApiResponse<CheckStockResponseDto>.FailureResult("Product not found"));
                }

                var distributorResponse = await _distributorApiService.CheckDistributorStockAsync(
                    product.DistributorProductId,
                    request.QuantityNeeded);

                var response = new CheckStockResponseDto
                {
                    ProductId = request.ProductId,
                    ProductName = product.ProductName,
                    CurrentSellerStock = product.CurrentStock,
                    QuantityNeeded = request.QuantityNeeded,
                    DistributorHasStock = distributorResponse.Success,
                    Message = distributorResponse.Message ?? "Stock check completed"
                };

                return Ok(ApiResponse<CheckStockResponseDto>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking distributor stock");
                return BadRequest(ApiResponse<CheckStockResponseDto>.FailureResult("Error checking stock"));
            }
        }

        [HttpPost("create-distributor-order")]
        public async Task<IActionResult> CreateDistributorOrder([FromBody] CreateDistributorOrderDto request)
        {
            try
            {
                // Validate products exist
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _context.SellerProducts
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                if (products.Count != request.Items.Count)
                {
                    return BadRequest(ApiResponse<bool>.FailureResult("One or more products not found"));
                }

                // Convert to distributor order items
                var distributorItems = request.Items.Select(item =>
                {
                    var product = products[item.ProductId];
                    return new DistributorOrderItem
                    {
                        DistributorProductId = product.DistributorProductId,
                        Quantity = item.Quantity,
                        RequestedPrice = product.PurchasePrice
                    };
                }).ToList();

                // Create order with distributor
                var result = await _distributorApiService.CreateDistributorOrderAsync(distributorItems);

                if (result.Success)
                {
                    // Log the order in our system for tracking
                    _logger.LogInformation($"Distributor order created for {distributorItems.Count} items");
                }

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return BadRequest(ApiResponse<bool>.FailureResult("Error creating distributor order"));
            }
        }

        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            try
            {
                var product = await _context.SellerProducts.FindAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponse<bool>.FailureResult("Product not found"));
                }

                product.CurrentStock = dto.NewStock;
                product.DisplayStock = Math.Min(dto.NewStock, product.DisplayStock);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Stock updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock");
                return BadRequest(ApiResponse<bool>.FailureResult("Error updating stock"));
            }
        }
    }

    
}