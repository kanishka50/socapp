using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs.Distributor;

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

        // NEW: Distributor Products for Order Creation
        Task<ApiResponse<PagedResult<DistributorProductDto>>> GetDistributorProductsAsync(PagedRequest request);
        Task<ApiResponse<DistributorProductDto>> GetDistributorProductByIdAsync(int id);


        Task<ApiResponse<bool>> CreateDistributorOrderAsync(CreateDistributorOrderRequest orderRequest);

        // Cart - Updated to return CartDto
        Task<ApiResponse<CartDto>> GetCartAsync(string sessionId);
        Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, AddToCartDto dto);
        Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, int productId);
        Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity);
        Task<ApiResponse<bool>> ClearCartAsync(string sessionId);

        // Authentication
        Task<ApiResponse<bool>> RegisterCustomerAsync(RegisterCustomerDto dto);


        // Inventory Management (NEW)
        Task<ApiResponse<PagedResult<SellerInventoryDto>>> GetInventoryAsync(PagedRequest request);
        Task<ApiResponse<CheckStockResponseDto>> CheckDistributorStockAsync(int productId, int quantityNeeded);
        Task<ApiResponse<bool>> CreateDistributorOrderAsync(CreateDistributorOrderDto request);
        Task<ApiResponse<bool>> UpdateStockAsync(int productId, int newStock);


        Task<ApiResponse<List<SellerDistributorOrderDto>>> GetDistributorOrdersAsync();
        Task<ApiResponse<SellerDistributorOrderDto>> CreateDistributorOrderAsync(CreateSellerDistributorOrderDto dto);
    }
}