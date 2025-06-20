using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Manufacturer;

namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface IManufacturerService
    {
        Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);
        Task<ApiResponse<StockCheckResponse>> CheckStockAsync(StockCheckRequest request);
    }
}