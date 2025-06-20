using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface ISellerService
    {
        Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<SellerProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<CartDto>> GetCartAsync(string sessionId);
        Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, AddToCartDto dto);
        Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity);
        Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, int productId);
        Task<ApiResponse<bool>> ClearCartAsync(string sessionId);
        Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto);
        Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string email);
    }
}