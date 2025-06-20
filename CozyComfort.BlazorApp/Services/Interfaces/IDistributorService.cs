using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;

namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface IDistributorService
    {
        Task<ApiResponse<PagedResult<DistributorProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<DistributorProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto);
        Task<ApiResponse<OrderDto>> ProcessSellerOrderAsync(ProcessSellerOrderDto dto);
        Task<ApiResponse<DistributorStockCheckResponse>> CheckStockAsync(DistributorStockCheckRequest request);
        Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string status);
    }
}