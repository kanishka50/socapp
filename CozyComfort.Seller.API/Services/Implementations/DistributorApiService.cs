using System.Net.Http.Json;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class DistributorApiService : IDistributorApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DistributorApiService> _logger;

        public DistributorApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<DistributorApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DistributorAPI");
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> CheckDistributorStockAsync(int distributorProductId, int quantity)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/products/check-stock", new
                {
                    ProductId = distributorProductId,
                    QuantityRequested = quantity
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<dynamic>>();
                    return ApiResponse<bool>.SuccessResult(true);
                }

                return ApiResponse<bool>.FailureResult("Stock check failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking distributor stock");
                return ApiResponse<bool>.FailureResult("Error checking stock");
            }
        }

        public async Task<ApiResponse<bool>> CreateDistributorOrderAsync(List<DistributorOrderItem> items)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get seller ID from configuration or current context
                var sellerId = _configuration.GetValue<int>("SellerInfo:SellerId", 1); // Default to 1 if not configured

                var orderDto = new ProcessSellerOrderDto
                {
                    SellerId = sellerId,
                    SellerOrderNumber = GenerateOrderNumber(),
                    Items = items.Select(item => new SellerOrderItem
                    {
                        DistributorProductId = item.DistributorProductId,
                        Quantity = item.Quantity,
                        RequestedPrice = item.RequestedPrice
                    }).ToList(),
                    ShippingAddress = _configuration["SellerInfo:ShippingAddress"] ?? "Default Seller Warehouse"
                };

                var response = await _httpClient.PostAsJsonAsync("api/orders/process-seller-order", orderDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
                    _logger.LogInformation($"Distributor order created successfully: {result?.Data?.OrderNumber}");
                    return ApiResponse<bool>.SuccessResult(true, "Order placed successfully with distributor");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to create distributor order. Status: {response.StatusCode}, Content: {errorContent}");
                return ApiResponse<bool>.FailureResult($"Failed to create distributor order: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return ApiResponse<bool>.FailureResult($"Error creating distributor order: {ex.Message}");
            }
        }

        public async Task<string> GetAuthTokenAsync()
        {
            try
            {
                // Check if we have a cached token (in production, implement proper token caching)
                // For now, we'll get a new token each time

                var loginResponse = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    Email = _configuration["DistributorApi:Email"] ?? "seller-api@cozycomfort.com",
                    Password = _configuration["DistributorApi:Password"] ?? "SellerAPI123!"
                });

                if (loginResponse.IsSuccessStatusCode)
                {
                    var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
                    return result?.Data?.Token ?? string.Empty;
                }

                _logger.LogError($"Failed to authenticate with distributor API: {loginResponse.StatusCode}");
                throw new Exception("Failed to authenticate with distributor API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auth token from distributor API");
                throw;
            }
        }


        public async Task<ApiResponse<PagedResult<DistributorProductDto>>> GetDistributorProductsAsync(PagedRequest request)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var query = $"api/products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<DistributorProductDto>>>(query);
                return response ?? new ApiResponse<PagedResult<DistributorProductDto>>
                {
                    Success = false,
                    Message = "No response from distributor API"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distributor products");
                return new ApiResponse<PagedResult<DistributorProductDto>>
                {
                    Success = false,
                    Message = "Error fetching distributor products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> GetDistributorProductByIdAsync(int id)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<DistributorProductDto>>($"api/products/{id}");
                return response ?? new ApiResponse<DistributorProductDto>
                {
                    Success = false,
                    Message = "Distributor product not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distributor product {ProductId}", id);
                return new ApiResponse<DistributorProductDto>
                {
                    Success = false,
                    Message = "Error fetching distributor product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private string GenerateOrderNumber()
        {
            return $"SEL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}