using CozyComfort.BlazorApp.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Manufacturer.API.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ManufacturerService> _logger;

        public ManufacturerService(IHttpClientFactory httpClientFactory, ILogger<ManufacturerService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ManufacturerAPI");
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var query = $"api/products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={request.SearchTerm}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>(query);
                return response ?? new ApiResponse<PagedResult<ProductDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return new ApiResponse<PagedResult<ProductDto>>
                {
                    Success = false,
                    Message = "Error fetching products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<ProductDto>>($"api/products/{id}");
                return response ?? new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", id);
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error fetching product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products", dto);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error creating product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<StockCheckResponse>> CheckStockAsync(StockCheckRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products/check-stock", request);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<StockCheckResponse>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock");
                return new ApiResponse<StockCheckResponse>
                {
                    Success = false,
                    Message = "Error checking stock",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Implement remaining methods
        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            // Implementation
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            // Implementation
            throw new NotImplementedException();
        }
    }
}