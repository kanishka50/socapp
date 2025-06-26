using CozyComfort.Shared.DTOs.Manufacturer;

namespace CozyComfort.Distributor.API.Services.Interfaces
{
    public interface IManufacturerApiService
    {
        Task<bool> CheckManufacturerStockAsync(int productId, int quantity);
        Task<string> GetAuthTokenAsync();
        Task<ProductDto> GetManufacturerProductByIdAsync(int productId);
    }
}