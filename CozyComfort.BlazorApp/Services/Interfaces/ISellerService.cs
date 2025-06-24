using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface ISellerService
    {
        // Products
        Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<SellerProductDto>> GetProductByIdAsync(int id);

        // Orders
        Task<ApiResponse<PagedResult<CustomerOrderDto>>> GetOrdersAsync(PagedRequest request);
        Task<ApiResponse<CustomerOrderDto>> GetOrderByIdAsync(int id);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string status);
        Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto);
        Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string customerEmail);

        // Cart
        Task<ApiResponse<CartDto>> GetCartAsync(string sessionId);
        Task<ApiResponse<bool>> AddToCartAsync(string sessionId, AddToCartDto dto);
        Task<ApiResponse<bool>> RemoveFromCartAsync(string sessionId, int productId);
        Task<ApiResponse<bool>> UpdateCartItemAsync(string sessionId, int productId, int quantity);
        Task<ApiResponse<bool>> ClearCartAsync(string sessionId);

        // Authentication
        Task<ApiResponse<bool>> RegisterCustomerAsync(RegisterCustomerDto dto);
    }
}