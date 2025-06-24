using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.BlazorApp.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class DistributorService : IDistributorService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly ILogger<DistributorService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DistributorService(HttpClient httpClient, IAuthService authService, ILogger<DistributorService> logger)
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

        public async Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto)
        {
            try
            {
                await SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Orders/manufacturer", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<OrderDto>.FailureResult("Failed to deserialize response");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error creating manufacturer order. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                return ApiResponse<OrderDto>.FailureResult($"Error: {response.StatusCode}");
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

                var response = await _httpClient.PostAsync("api/Orders/seller", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(responseContent, _jsonOptions);
                    return result ?? ApiResponse<OrderDto>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<OrderDto>.FailureResult($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing seller order");
                return ApiResponse<OrderDto>.FailureResult($"Error: {ex.Message}");
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

        public async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize)
        {
            try
            {
                await SetAuthorizationHeader();  // Use existing method

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

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                await SetAuthorizationHeader();  // Use existing method

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
    }
}