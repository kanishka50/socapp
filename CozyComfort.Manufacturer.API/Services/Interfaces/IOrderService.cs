
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Manufacturer;

namespace CozyComfort.Manufacturer.API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<PagedResult<ManufacturerOrderDto>>> GetOrdersAsync(int pageNumber, int pageSize);
        Task<ApiResponse<ManufacturerOrderDto>> GetOrderByIdAsync(int id);
        Task<ApiResponse<ManufacturerOrderDto>> CreateOrderFromDistributorAsync(CreateManufacturerOrderFromDistributorDto dto);
        Task<ApiResponse<ManufacturerOrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
        Task<ApiResponse<List<ManufacturerInventoryDto>>> GetInventoryAsync();
    }
}