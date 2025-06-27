using System.Net.Http.Json;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Seller;

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

        public async Task<ApiResponse<OrderDto>> CreateDistributorOrderAsync(List<DistributorOrderItemDto> items)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var orderRequest = new ProcessSellerOrderDto
                {
                    SellerId = 1, // Should come from current user context
                    SellerOrderNumber = $"SEL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    ShippingAddress = "Default Seller Address", // Should come from seller profile
                    Items = items.Select(i => new ProcessSellerOrderItemDto
                    {
                        DistributorProductId = i.DistributorProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync("api/orders/from-seller", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
                    return result ?? ApiResponse<OrderDto>.FailureResult("Failed to parse response");
                }

                var error = await response.Content.ReadAsStringAsync();
                return ApiResponse<OrderDto>.FailureResult($"Failed to create order: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return ApiResponse<OrderDto>.FailureResult("Error creating order");
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> GetDistributorProductByIdAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<DistributorProductDto>>();
                    return result ?? ApiResponse<DistributorProductDto>.FailureResult("Failed to parse response");
                }

                return ApiResponse<DistributorProductDto>.FailureResult("Product not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor product");
                return ApiResponse<DistributorProductDto>.FailureResult("Error retrieving product");
            }
        }

        public async Task<ApiResponse<PagedResult<DistributorProductDto>>> GetDistributorProductsAsync(PagedRequest request)
        {
            try
            {
                var queryParams = $"?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    queryParams += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                var response = await _httpClient.GetAsync($"api/products{queryParams}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<DistributorProductDto>>>();
                    return result ?? ApiResponse<PagedResult<DistributorProductDto>>.FailureResult("Failed to parse response");
                }

                return ApiResponse<PagedResult<DistributorProductDto>>.FailureResult("Failed to retrieve products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor products");
                return ApiResponse<PagedResult<DistributorProductDto>>.FailureResult("Error retrieving products");
            }
        }

        private async Task<string> GetAuthTokenAsync()
        {
            try
            {
                var loginRequest = new
                {
                    Email = "seller-api@cozycomfort.com",
                    Password = "SellerAPI123!"
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                    return result?.Data?.Token ?? string.Empty;
                }

                _logger.LogError("Failed to authenticate with distributor API");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auth token");
                return string.Empty;
            }
        }
    }

    // Internal DTOs for DistributorApiService
   

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}