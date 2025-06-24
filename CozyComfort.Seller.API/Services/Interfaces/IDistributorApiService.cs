using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface IDistributorApiService
    {
        Task<ApiResponse<bool>> CheckDistributorStockAsync(int distributorProductId, int quantity);
        Task<ApiResponse<bool>> CreateDistributorOrderAsync(List<DistributorOrderItem> items);
        Task<string> GetAuthTokenAsync();
    }
}