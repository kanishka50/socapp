using CozyComfort.Distributor.API.Services.Interfaces;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class ManufacturerApiService : IManufacturerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ManufacturerApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ManufacturerAPI");
            _configuration = configuration;
        }

        public async Task<bool> CheckManufacturerStockAsync(int productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetAuthTokenAsync()
        {
            throw new NotImplementedException();
        }
    }
}