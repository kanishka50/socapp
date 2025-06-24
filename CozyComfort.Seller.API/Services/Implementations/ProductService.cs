using Microsoft.EntityFrameworkCore;
using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly SellerDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(SellerDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var query = _context.SellerProducts.Where(p => p.IsActive && p.IsAvailable);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(p =>
                        p.ProductName.Contains(request.SearchTerm) ||
                        p.Description.Contains(request.SearchTerm) ||
                        p.Category.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new SellerProductDto
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Price = p.SellingPrice,
                        ImageUrl = p.ImageUrl,
                        InStock = p.DisplayStock > 0
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<SellerProductDto>
                {
                    Items = products,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResult<SellerProductDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return ApiResponse<PagedResult<SellerProductDto>>.FailureResult("Error retrieving products");
            }
        }

        public async Task<ApiResponse<SellerProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.SellerProducts
                    .Where(p => p.Id == id && p.IsActive && p.IsAvailable)
                    .Select(p => new SellerProductDto
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Price = p.SellingPrice,
                        ImageUrl = p.ImageUrl,
                        InStock = p.DisplayStock > 0
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return ApiResponse<SellerProductDto>.FailureResult("Product not found");
                }

                return ApiResponse<SellerProductDto>.SuccessResult(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product");
                return ApiResponse<SellerProductDto>.FailureResult("Error retrieving product");
            }
        }
    }
}