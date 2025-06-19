using CozyComfort.Distributor.API.Models.DTOs;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Distributor.API.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResult<DistributorProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<DistributorProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<DistributorProductDto>> AddProductFromManufacturerAsync(CreateDistributorProductDto dto);
        Task<ApiResponse<DistributorProductDto>> UpdateProductAsync(int id, UpdateDistributorProductDto dto);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);
        Task<ApiResponse<DistributorStockCheckResponse>> CheckStockAsync(DistributorStockCheckRequest request);
        Task<ApiResponse<bool>> UpdateStockAsync(int productId, UpdateDistributorStockDto dto);
    }
}