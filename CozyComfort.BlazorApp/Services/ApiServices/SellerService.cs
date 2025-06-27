using Blazored.LocalStorage;
using CozyComfort.BlazorApp.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Seller;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services.ApiServices
{
    public class SellerService : ISellerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SellerService> _logger;
        private readonly ILocalStorageService _localStorage;

        public SellerService(IHttpClientFactory httpClientFactory, ILogger<SellerService> logger, ILocalStorageService localStorage)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localStorage = localStorage;
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            var client = _httpClientFactory.CreateClient("SellerAPI");
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Token set for request: {Token}", token.Substring(0, 20) + "...");
            }
            else
            {
                _logger.LogWarning("No auth token found in localStorage");
            }

            return client;
        }

        public async Task<ApiResponse<CombinedOrdersDto>> GetCombinedOrdersAsync(PagedRequest request)
        {
            try
            {
                var queryParams = $"?PageNumber={request.PageNumber}&PageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    queryParams += $"&SearchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                var response = await _httpClient.GetAsync($"api/orders/combined{queryParams}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CombinedOrdersDto>>();
                    return result ?? ApiResponse<CombinedOrdersDto>.FailureResult("Failed to deserialize response");
                }

                return ApiResponse<CombinedOrdersDto>.FailureResult($"Failed to load orders: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combined orders");
                return ApiResponse<CombinedOrdersDto>.FailureResult("Failed to load orders");
            }
        }

        #region Products

        public async Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var query = $"api/Products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<SellerProductDto>>>(query);
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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.GetFromJsonAsync<ApiResponse<SellerProductDto>>($"api/Products/{id}");
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
                var client = await GetAuthenticatedClientAsync();

                var query = $"api/orders?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                _logger.LogInformation("Fetching orders from: {Query}", query);

                var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<CustomerOrderDto>>>(query);
                return response ?? new ApiResponse<PagedResult<CustomerOrderDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error fetching orders: {StatusCode}", httpEx.StatusCode);
                return new ApiResponse<PagedResult<CustomerOrderDto>>
                {
                    Success = false,
                    Message = $"Error fetching orders: {httpEx.Message}",
                    Errors = new List<string> { httpEx.Message }
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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.GetFromJsonAsync<ApiResponse<CustomerOrderDto>>($"api/orders/{id}");
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
                var client = await GetAuthenticatedClientAsync();

                var updateDto = new { Status = status };
                var response = await client.PutAsJsonAsync($"api/orders/{orderId}/status", updateDto);

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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.PostAsJsonAsync("api/orders/create", dto);

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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.GetFromJsonAsync<ApiResponse<List<CustomerOrderDto>>>($"api/orders/customer/{Uri.EscapeDataString(customerEmail)}");
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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.GetFromJsonAsync<ApiResponse<CartDto>>($"api/cart/{sessionId}");
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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.PostAsJsonAsync($"api/cart/{sessionId}/add", dto);

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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.DeleteAsync($"api/cart/{sessionId}/remove/{productId}");

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

        public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, int productId, int quantity)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var updateDto = new { ProductId = productId, Quantity = quantity };
                var response = await client.PutAsJsonAsync($"api/cart/{sessionId}/update", updateDto);

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
                var client = await GetAuthenticatedClientAsync();

                var response = await client.DeleteAsync($"api/cart/{sessionId}/clear");

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
                var client = _httpClientFactory.CreateClient("SellerAPI");

                var response = await client.PostAsJsonAsync("api/auth/register-customer", dto);

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


        public async Task<ApiResponse<PagedResult<SellerInventoryDto>>> GetInventoryAsync(PagedRequest request)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var query = $"api/inventory?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<SellerInventoryDto>>>(query);
                return response ?? new ApiResponse<PagedResult<SellerInventoryDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory");
                return new ApiResponse<PagedResult<SellerInventoryDto>>
                {
                    Success = false,
                    Message = "Error fetching inventory",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<CheckStockResponseDto>> CheckDistributorStockAsync(int productId, int quantityNeeded)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var checkRequest = new
                {
                    ProductId = productId,
                    QuantityNeeded = quantityNeeded
                };

                var response = await client.PostAsJsonAsync("api/inventory/check-distributor-stock", checkRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CheckStockResponseDto>>();
                    return result ?? new ApiResponse<CheckStockResponseDto>
                    {
                        Success = false,
                        Message = "Invalid response"
                    };
                }

                return new ApiResponse<CheckStockResponseDto>
                {
                    Success = false,
                    Message = $"Failed to check stock: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking distributor stock");
                return new ApiResponse<CheckStockResponseDto>
                {
                    Success = false,
                    Message = "Error checking distributor stock",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateDistributorOrderAsync(CreateDistributorOrderDto request)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var response = await client.PostAsJsonAsync("api/inventory/create-distributor-order", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? new ApiResponse<bool> { Success = true, Data = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to create distributor order: {response.StatusCode}, Content: {errorContent}");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to create distributor order: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error creating distributor order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateStockAsync(int productId, int newStock)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var updateDto = new { NewStock = newStock };
                var response = await client.PutAsJsonAsync($"api/inventory/update-stock/{productId}", updateDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? new ApiResponse<bool> { Success = true, Data = true };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to update stock: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating stock",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResult<DistributorProductDto>>> GetDistributorProductsAsync(PagedRequest request)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var query = $"api/distributor/products?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    query += $"&sortBy={request.SortBy}&isDescending={request.IsDescending}";

                var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<DistributorProductDto>>>(query);
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
                    Message = "Error fetching distributor products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DistributorProductDto>> GetDistributorProductByIdAsync(int id)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var response = await client.GetFromJsonAsync<ApiResponse<DistributorProductDto>>($"api/distributor/products/{id}");
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

        public async Task<ApiResponse<bool>> CreateDistributorOrderAsync(CreateDistributorOrderRequest orderRequest)
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();

                var response = await client.PostAsJsonAsync("api/distributor-orders/create", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? new ApiResponse<bool> { Success = true, Data = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to create distributor order: {response.StatusCode}, Content: {errorContent}");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to create distributor order: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error creating distributor order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion
    }
}