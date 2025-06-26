using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> UpdateStockAfterOrderAsync(int productId, int quantity);
        Task<bool> CheckStockAvailabilityAsync(int productId, int quantity);
        Task<ApiResponse<bool>> UpdateStockBulkAsync(UpdateSellerStockBulkDto dto);
    }
}