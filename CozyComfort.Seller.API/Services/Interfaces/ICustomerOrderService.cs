using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface ICustomerOrderService
    {
        Task<ApiResponse<PagedResult<CustomerOrderDto>>> GetOrdersAsync(PagedRequest request);
        Task<ApiResponse<CustomerOrderDto>> GetOrderByIdAsync(int id);
        Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status);
        Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string customerEmail);
    }
}