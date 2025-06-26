using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Manufacturer;
using System.Net.Http.Json;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class ManufacturerApiService : IManufacturerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ManufacturerApiService> _logger;

        public ManufacturerApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ManufacturerApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ManufacturerAPI");
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ProductDto> GetManufacturerProductByIdAsync(int productId)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<ProductDto>>($"api/products/{productId}");
                return response?.Success == true ? response.Data : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching manufacturer product {ProductId}", productId);
                return null;
            }
        }

        public async Task<string> GetAuthTokenAsync()
        {
            try
            {
                var loginRequest = new
                {
                    Email = "distributor-api@cozycomfort.com",
                    Password = "DistributorAPI123!"
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
                    return result?.Data?.Token ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturer auth token");
                return string.Empty;
            }
        }

        public async Task<bool> CheckManufacturerStockAsync(int productId, int quantity)
        {
            throw new NotImplementedException();
        }
    }
}