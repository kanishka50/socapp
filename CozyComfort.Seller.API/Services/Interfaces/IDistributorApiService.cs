//using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;


namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface IDistributorApiService
    {
        Task<ApiResponse<bool>> CheckDistributorStockAsync(int distributorProductId, int quantity);
        Task<ApiResponse<bool>> CreateDistributorOrderAsync(List<DistributorOrderItem> items);
        Task<string> GetAuthTokenAsync();
    }
}