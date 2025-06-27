using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface ISellerOrderService
    {
        Task<ApiResponse<SellerDistributorOrderDto>> CreateDistributorOrderAsync(CreateSellerDistributorOrderDto dto);
        Task<ApiResponse<PagedResult<SellerDistributorOrderDto>>> GetDistributorOrdersAsync(PagedRequest request);
        Task<ApiResponse<SellerDistributorOrderDto>> GetDistributorOrderByIdAsync(int orderId);
        Task<ApiResponse<bool>> UpdateOrderFromDistributorAcceptanceAsync(string distributorOrderNumber);
        Task<ApiResponse<CombinedOrdersDto>> GetCombinedOrdersAsync(PagedRequest request);
    }
}