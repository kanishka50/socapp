using CozyComfort.BlazorApp.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using System.Net.Http.Json;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class SellerService : ISellerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SellerService> _logger;

        public SellerService(HttpClient httpClient, ILogger<SellerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var query = $"api/products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={request.SearchTerm}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<SellerProductDto>>>(query);
                return response ?? new ApiResponse<PagedResult<SellerProductDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return new ApiResponse<PagedResult<SellerProductDto>>
                {
                    Success = false,
                    Message = "Error fetching products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<SellerProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<SellerProductDto>>($"api/products/{id}");
                return response ?? new ApiResponse<SellerProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", id);
                return new ApiResponse<SellerProductDto>
                {
                    Success = false,
                    Message = "Error fetching product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CartDto>> GetCartAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CartDto>>($"api/cart/{sessionId}");
                return response ?? new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Cart not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cart");
                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Error fetching cart",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, AddToCartDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/cart/{sessionId}/add", dto);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<CartDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<CartDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Error adding to cart",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"api/cart/{sessionId}/update",
                    new { ProductId = productId, Quantity = quantity });

                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<CartDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<CartDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Error updating cart item",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, int productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cart/{sessionId}/remove/{productId}");
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<CartDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<CartDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = "Error removing from cart",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cart/{sessionId}/clear");
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<bool>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<bool> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error clearing cart",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/orders/create", dto);
                var content = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponse<CustomerOrderDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new ApiResponse<CustomerOrderDto> { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = "Error creating order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CustomerOrderDto>>>(
                    $"api/orders/customer/{email}");

                return response ?? new ApiResponse<List<CustomerOrderDto>>
                {
                    Success = false,
                    Message = "No orders found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer orders");
                return new ApiResponse<List<CustomerOrderDto>>
                {
                    Success = false,
                    Message = "Error fetching orders",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}