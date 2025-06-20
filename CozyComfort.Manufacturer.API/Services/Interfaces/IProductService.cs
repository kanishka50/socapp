using System.Threading.Tasks;
//using CozyComfort.Manufacturer.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Manufacturer;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Manufacturer.API.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);
        Task<ApiResponse<StockCheckResponse>> CheckStockAsync(StockCheckRequest request);
        Task<ApiResponse<bool>> UpdateStockAsync(int productId, UpdateStockDto dto);
    }
}