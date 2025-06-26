using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Distributor.API.Services.Interfaces
{
    public interface ISellerApiService
    {
        Task<ApiResponse<bool>> UpdateSellerStockAsync(UpdateSellerStockBulkDto dto);
    }
}