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

        public SellerService(IHttpClientFactory httpClientFactory, ILogger<SellerService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("SellerAPI");
            _logger = logger;
        }

        #region Products

        public async Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                // Add debugging
                _logger.LogInformation("HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);

                var query = $"api/Products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                // Add debugging
                _logger.LogInformation("Full URL: {Url}", $"{_httpClient.BaseAddress}{query}");

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<SellerProductDto>>>(query);
                return response ?? new ApiResponse<PagedResult<SellerProductDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching seller products");
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
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<SellerProductDto>>($"api/Products/{id}");
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

        #endregion

        #region Orders

        public async Task<ApiResponse<PagedResult<CustomerOrderDto>>> GetOrdersAsync(PagedRequest request)
        {
            try
            {
                var query = $"api/orders?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<CustomerOrderDto>>>(query);
                return response ?? new ApiResponse<PagedResult<CustomerOrderDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return new ApiResponse<PagedResult<CustomerOrderDto>>
                {
                    Success = false,
                    Message = "Error fetching orders",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CustomerOrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerOrderDto>>($"api/orders/{id}");
                return response ?? new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", id);
                return new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = "Error fetching order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var updateDto = new { Status = status };
                var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/status", updateDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
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

        public async Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/orders/create", dto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerOrderDto>>();
                    return result ?? new ApiResponse<CustomerOrderDto>
                    {
                        Success = false,
                        Message = "Invalid response"
                    };
                }

                return new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = $"Failed to create order: {response.StatusCode}"
                };
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

        public async Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string customerEmail)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CustomerOrderDto>>>($"api/orders/customer/{Uri.EscapeDataString(customerEmail)}");
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
                    Message = "Error fetching customer orders",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Cart

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

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CartDto>>();
                    return result ?? new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Invalid response"
                    };
                }

                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = $"Failed to add to cart: {response.StatusCode}"
                };
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

        public async Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, int productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cart/{sessionId}/remove/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CartDto>>();
                    return result ?? new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Invalid response"
                    };
                }

                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = $"Failed to remove from cart: {response.StatusCode}"
                };
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

        // Additional cart methods that might be called
        public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity)
        {
            try
            {
                var updateDto = new { ProductId = productId, Quantity = quantity };
                var response = await _httpClient.PutAsJsonAsync($"api/cart/{sessionId}/update", updateDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CartDto>>();
                    return result ?? new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Invalid response"
                    };
                }

                return new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = $"Failed to update cart item: {response.StatusCode}"
                };
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

        public async Task<ApiResponse<bool>> ClearCartAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cart/{sessionId}/clear");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? new ApiResponse<bool> { Success = true, Data = true };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to clear cart: {response.StatusCode}"
                };
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

        #endregion

        #region Authentication

        public async Task<ApiResponse<bool>> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register-customer", dto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? new ApiResponse<bool> { Success = true, Data = true };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to register customer: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering customer");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error registering customer",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion
    }
}