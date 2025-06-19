using System.Net.Http.Json;
using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

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
            // Implementation
            throw new NotImplementedException();
        }

        public async Task<string> GetAuthTokenAsync()
        {
            // Login to distributor API and get token
            // This is simplified - in production, you'd cache the token
            var loginResponse = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                Email = "seller-api@cozycomfort.com",
                Password = "SellerAPI123!"
            });

            if (loginResponse.IsSuccessStatusCode)
            {
                var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
                return result?.Data?.Token ?? string.Empty;
            }

            throw new Exception("Failed to authenticate with distributor API");
        }
    }
}