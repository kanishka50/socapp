using System.Net.Http.Json;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class SellerApiService : ISellerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SellerApiService> _logger;

        public SellerApiService(IHttpClientFactory httpClientFactory, ILogger<SellerApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("SellerAPI");
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> UpdateSellerStockAsync(UpdateSellerStockBulkDto dto)
        {
            try
            {
                // Add API key for service-to-service authentication
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", "DistributorAPIKey123");

                var response = await _httpClient.PostAsJsonAsync("api/stock/update-bulk", dto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? ApiResponse<bool>.FailureResult("Invalid response");
                }

                return ApiResponse<bool>.FailureResult($"Failed to update seller stock: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating seller stock");
                return ApiResponse<bool>.FailureResult($"Error updating seller stock: {ex.Message}");
            }
        }
    }
}