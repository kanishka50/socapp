using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface IDistributorApiService
    {
        Task<ApiResponse<bool>> CheckDistributorStockAsync(int distributorProductId, int quantity);
        Task<ApiResponse<bool>> CreateDistributorOrderAsync(List<DistributorOrderItem> items);
        Task<ApiResponse<OrderDto>> CreateDistributorOrderAsync(ProcessSellerOrderDto dto);
        Task<string> GetAuthTokenAsync();
        // Distributor Products
        Task<ApiResponse<PagedResult<DistributorProductDto>>> GetDistributorProductsAsync(PagedRequest request);
        Task<ApiResponse<DistributorProductDto>> GetDistributorProductByIdAsync(int id);
    }
}