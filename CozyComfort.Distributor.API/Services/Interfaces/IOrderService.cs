//using CozyComfort.Distributor.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Distributor.API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize);
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id);
        Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto);
        Task<ApiResponse<OrderDto>> ProcessSellerOrderAsync(ProcessSellerOrderDto dto);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status);
    }
}