using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Manufacturer;
using CozyComfort.BlazorApp.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly ILogger<ManufacturerService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ManufacturerService(HttpClient httpClient, IAuthService authService, ILogger<ManufacturerService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                await SetAuthorizationHeader();

                var queryParams = new List<string>
                {
                    $"pageNumber={request.PageNumber}",
                    $"pageSize={request.PageSize}"
                };

                if (!string.IsNullOrEmpty(request.SearchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
                if (!string.IsNullOrEmpty(request.SortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");
                if (request.IsDescending)
                    queryParams.Add($"isDescending=true");

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/Products?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductDto>>>(content, _jsonOptions);
                    return result ?? ApiResponse<PagedResult<ProductDto>>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<PagedResult<ProductDto>>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return ApiResponse<PagedResult<ProductDto>>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/Products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, _jsonOptions);
                    return result ?? ApiResponse<ProductDto>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<ProductDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return ApiResponse<ProductDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                await SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Products", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<ProductDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error creating product. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<ProductDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ApiResponse<ProductDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            try
            {
                await SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/Products/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<ProductDto>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<ProductDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return ApiResponse<ProductDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/Products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                    return result ?? ApiResponse<bool>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<bool>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return ApiResponse<bool>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StockCheckResponse>> CheckStockAsync(StockCheckRequest request)
        {
            try
            {
                await SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Products/check-stock", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<StockCheckResponse>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<StockCheckResponse>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<StockCheckResponse>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock");
                return ApiResponse<StockCheckResponse>.FailureResult($"Error: {ex.Message}");
            }
        }
    }
}