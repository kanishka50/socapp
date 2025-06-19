using Microsoft.Extensions.Caching.Memory;
using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly IMemoryCache _cache;
        private readonly SellerDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(IMemoryCache cache, SellerDbContext context, ILogger<CartService> logger)
        {
            _cache = cache;
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<CartDto>> GetCartAsync(string sessionId)
        {
            try
            {
                var cart = _cache.Get<CartDto>($"cart_{sessionId}") ?? new CartDto { SessionId = sessionId };
                return ApiResponse<CartDto>.SuccessResult(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return ApiResponse<CartDto>.FailureResult("Error retrieving cart");
            }
        }

        public async Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, AddToCartDto dto)
        {
            try
            {
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                {
                    return ApiResponse<CartDto>.FailureResult("Product not found");
                }

                var cart = _cache.Get<CartDto>($"cart_{sessionId}") ?? new CartDto { SessionId = sessionId };

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
                }
                else
                {
                    cart.Items.Add(new CartItemDto
                    {
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        SKU = product.SKU,
                        UnitPrice = product.SellingPrice,
                        Quantity = dto.Quantity,
                        ImageUrl = product.ImageUrl
                    });
                }

                _cache.Set($"cart_{sessionId}", cart, TimeSpan.FromHours(24));
                return ApiResponse<CartDto>.SuccessResult(cart, "Product added to cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return ApiResponse<CartDto>.FailureResult("Error adding to cart");
            }
        }

        public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity)
        {
            try
            {
                var cart = _cache.Get<CartDto>($"cart_{sessionId}");
                if (cart == null)
                {
                    return ApiResponse<CartDto>.FailureResult("Cart not found");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    return ApiResponse<CartDto>.FailureResult("Item not found in cart");
                }

                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }

                _cache.Set($"cart_{sessionId}", cart, TimeSpan.FromHours(24));
                return ApiResponse<CartDto>.SuccessResult(cart, "Cart updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                return ApiResponse<CartDto>.FailureResult("Error updating cart");
            }
        }

        public async Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, int productId)
        {
            return await UpdateCartItemAsync(sessionId, productId, 0);
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(string sessionId)
        {
            try
            {
                _cache.Remove($"cart_{sessionId}");
                return ApiResponse<bool>.SuccessResult(true, "Cart cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return ApiResponse<bool>.FailureResult("Error clearing cart");
            }
        }
    }
}