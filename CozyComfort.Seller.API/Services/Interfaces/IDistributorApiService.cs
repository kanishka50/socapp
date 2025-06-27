using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Seller.API.Services.Implementations;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface IDistributorApiService
    {
        Task<ApiResponse<DistributorProductDto>> GetDistributorProductByIdAsync(int productId);
        Task<ApiResponse<PagedResult<DistributorProductDto>>> GetDistributorProductsAsync(PagedRequest request);
        Task<ApiResponse<bool>> CheckDistributorStockAsync(int productId, int quantity);
        Task<ApiResponse<OrderDto>> CreateDistributorOrderAsync(List<SellerDistributorOrderItemRequest> items);
    }
}