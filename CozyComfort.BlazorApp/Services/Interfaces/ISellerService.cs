using CozyComfort.Shared.DTOs;
using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.BlazorApp.Services.ApiServices;


namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface ISellerService
    {
        Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<SellerProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<CartDto>> GetCartAsync(string sessionId);
        Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, AddToCartDto dto);
        Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto);
    }
}