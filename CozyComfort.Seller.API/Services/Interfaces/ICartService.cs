//using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface ICartService
    {
        Task<ApiResponse<CartDto>> GetCartAsync(string sessionId);
        Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, AddToCartDto dto);
        Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity);
        Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, int productId);
        Task<ApiResponse<bool>> ClearCartAsync(string sessionId);
    }
}