using CozyComfort.BlazorApp.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using System.Net.Http.Json;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class DistributorService : IDistributorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DistributorService> _logger;

        public DistributorService(IHttpClientFactory httpClientFactory, ILogger<DistributorService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DistributorAPI");
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<DistributorProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var query = $"api/products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={request.SearchTerm}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<DistributorProductDto>>>(query);
                return response ?? new ApiResponse<PagedResult<DistributorProductDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distributor products");
                return new ApiResponse<PagedResult<DistributorProductDto>>
                {
                    Success = false,
                    Message = "Error fetching products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<DistributorProductDto>>($"api/products/{id}");
                return response ?? new ApiResponse<DistributorProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distributor product {ProductId}", id);
                return new ApiResponse<DistributorProductDto>
                {
                    Success = false,
                    Message = "Error fetching product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/orders/create-manufacturer-order", dto);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<OrderDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<OrderDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manufacturer order");
                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Error creating order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDto>> ProcessSellerOrderAsync(ProcessSellerOrderDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/orders/process-seller-order", dto);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<OrderDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<OrderDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing seller order");
                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Error processing order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DistributorStockCheckResponse>> CheckStockAsync(DistributorStockCheckRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products/check-stock", request);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<DistributorStockCheckResponse>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<DistributorStockCheckResponse> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock");
                return new ApiResponse<DistributorStockCheckResponse>
                {
                    Success = false,
                    Message = "Error checking stock",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<OrderDto>>>(
                    $"api/orders?pageNumber={pageNumber}&pageSize={pageSize}");

                return response ?? new ApiResponse<PagedResult<OrderDto>>
                {
                    Success = false,
                    Message = "No response from server"
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
                var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/update-status", new { Status = status });
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<bool>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<bool> { Success = false, Message = "Invalid response" };
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



        // File: CozyComfort.BlazorApp/Services/ApiServices/DistributorService.cs

        public async Task<ApiResponse<DistributorProductDto>> AddProductFromManufacturerAsync(CreateDistributorProductDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products/add-from-manufacturer", dto);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<DistributorProductDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<DistributorProductDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product from manufacturer");
                return new ApiResponse<DistributorProductDto>
                {
                    Success = false,
                    Message = "Error adding product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}