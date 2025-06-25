using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Manufacturer;
using CozyComfort.BlazorApp.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class DistributorService : IDistributorService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthService _authService;
        private readonly ILogger<DistributorService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DistributorService(
            HttpClient httpClient,
            IHttpClientFactory httpClientFactory,
            IAuthService authService,
            ILogger<DistributorService> logger)
        {
            _httpClient = httpClient;
            _httpClientFactory = httpClientFactory;
            _authService = authService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task SetAuthorizationHeader(HttpClient client = null)
        {
            var targetClient = client ?? _httpClient;
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                targetClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        #region Distributor Products Methods

        public async Task<ApiResponse<PagedResult<DistributorProductDto>>> GetProductsAsync(PagedRequest request)
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
                    var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<DistributorProductDto>>>(content, _jsonOptions);
                    return result ?? ApiResponse<PagedResult<DistributorProductDto>>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<PagedResult<DistributorProductDto>>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor products");
                return ApiResponse<PagedResult<DistributorProductDto>>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/Products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<DistributorProductDto>>(content, _jsonOptions);
                    return result ?? ApiResponse<DistributorProductDto>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<DistributorProductDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor product {ProductId}", id);
                return ApiResponse<DistributorProductDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DistributorStockCheckResponse>> CheckStockAsync(DistributorStockCheckRequest request)
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
                    var result = JsonSerializer.Deserialize<ApiResponse<DistributorStockCheckResponse>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<DistributorStockCheckResponse>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<DistributorStockCheckResponse>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock");
                return ApiResponse<DistributorStockCheckResponse>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> AddProductFromManufacturerAsync(CreateDistributorProductDto dto)
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
                    var result = JsonSerializer.Deserialize<ApiResponse<DistributorProductDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<DistributorProductDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error adding product from manufacturer. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<DistributorProductDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product from manufacturer");
                return ApiResponse<DistributorProductDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        #endregion

        #region Manufacturer Products Methods (New Addition)

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetManufacturerProductsAsync(PagedRequest request)
        {
            try
            {
                using var manufacturerClient = _httpClientFactory.CreateClient("ManufacturerAPI");
                await SetAuthorizationHeader(manufacturerClient);

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
                var response = await manufacturerClient.GetAsync($"api/products?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductDto>>>(content, _jsonOptions);
                    return result ?? ApiResponse<PagedResult<ProductDto>>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting manufacturer products. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<PagedResult<ProductDto>>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturer products");
                return ApiResponse<PagedResult<ProductDto>>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetManufacturerProductByIdAsync(int id)
        {
            try
            {
                using var manufacturerClient = _httpClientFactory.CreateClient("ManufacturerAPI");
                await SetAuthorizationHeader(manufacturerClient);

                var response = await manufacturerClient.GetAsync($"api/products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, _jsonOptions);
                    return result ?? ApiResponse<ProductDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting manufacturer product {ProductId}. Status: {Status}, Content: {Content}", id, response.StatusCode, errorContent);
                return ApiResponse<ProductDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturer product {ProductId}", id);
                return ApiResponse<ProductDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StockCheckResponse>> CheckManufacturerStockAsync(StockCheckRequest request)
        {
            try
            {
                using var manufacturerClient = _httpClientFactory.CreateClient("ManufacturerAPI");
                await SetAuthorizationHeader(manufacturerClient);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await manufacturerClient.PostAsync("api/products/check-stock", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<StockCheckResponse>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<StockCheckResponse>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error checking manufacturer stock. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<StockCheckResponse>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking manufacturer stock");
                return ApiResponse<StockCheckResponse>.FailureResult($"Error: {ex.Message}");
            }
        }

        #endregion

        #region Order Methods

        public async Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto)
        {
            try
            {
                await SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Orders/create-manufacturer-order", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<OrderDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error creating manufacturer order. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<OrderDto>.FailureResult($"Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manufacturer order");
                return ApiResponse<OrderDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OrderDto>> ProcessSellerOrderAsync(ProcessSellerOrderDto dto)
        {
            try
            {
                await SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Orders/process-seller-order", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<OrderDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error processing seller order. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<OrderDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing seller order");
                return ApiResponse<OrderDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize)
        {
            try
            {
                await SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"api/orders?pageNumber={pageNumber}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<OrderDto>>>(content, _jsonOptions);
                    return result ?? new ApiResponse<PagedResult<OrderDto>>
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching orders. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return new ApiResponse<PagedResult<OrderDto>>
                {
                    Success = false,
                    Message = $"Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return new ApiResponse<PagedResult<OrderDto>>
                {
                    Success = false,
                    Message = "Error fetching orders",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"api/orders/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(content, _jsonOptions);
                    return result ?? ApiResponse<OrderDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting order {OrderId}. Status: {Status}, Content: {Content}", id, response.StatusCode, errorContent);
                return ApiResponse<OrderDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", id);
                return ApiResponse<OrderDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                await SetAuthorizationHeader();

                var updateDto = new { Status = status };
                var json = JsonSerializer.Serialize(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/orders/{orderId}/update-status", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, _jsonOptions);
                    return result ?? new ApiResponse<bool> { Success = true, Data = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error updating order status. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to update order status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating order status",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion
    }
}