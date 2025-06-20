using Blazored.LocalStorage;
using CozyComfort.BlazorApp.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Enums;
using CozyComfort.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace CozyComfort.BlazorApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider,
            ILocalStorageService localStorage)
        {
            _httpClientFactory = httpClientFactory;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<TokenDto>> LoginAsync(string apiEndpoint, LoginDto loginDto)
        {
            try
            {
                HttpClient httpClient = apiEndpoint switch
                {
                    "manufacturer" => _httpClientFactory.CreateClient("ManufacturerAPI"),
                    "distributor" => _httpClientFactory.CreateClient("DistributorAPI"),
                    "seller" => _httpClientFactory.CreateClient("SellerAPI"),
                    _ => throw new ArgumentException("Invalid API endpoint")
                };

                var response = await httpClient.PostAsJsonAsync("api/auth/login", loginDto);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Login failed. Please check your credentials.",
                        Errors = new List<string> { content }
                    };
                }

                var result = JsonSerializer.Deserialize<ApiResponse<TokenDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Success && result.Data != null)
                {
                    // Store JWT token in local storage
                    await _localStorage.SetItemAsync("authToken", result.Data.Token);
                    await _localStorage.SetItemAsync("userRole", result.Data.Role);
                    await _localStorage.SetItemAsync("apiEndpoint", apiEndpoint);

                    // Notify the custom auth state provider
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
                }

                return result ?? new ApiResponse<TokenDto> { Success = false, Message = "Invalid response from server" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<TokenDto>
                {
                    Success = false,
                    Message = "Unable to connect to the server. Please ensure the API is running.",
                    Errors = new List<string> { ex.Message }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenDto>
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userRole");
            await _localStorage.RemoveItemAsync("apiEndpoint");

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrWhiteSpace(token);
        }

        public async Task<string> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                return new User
                {
                    Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                    FirstName = user.FindFirst(ClaimTypes.Name)?.Value?.Split(' ')[0] ?? string.Empty,
                    LastName = user.FindFirst(ClaimTypes.Name)?.Value?.Split(' ').LastOrDefault() ?? string.Empty,
                    Role = Enum.TryParse<UserRole>(user.FindFirst(ClaimTypes.Role)?.Value, out var role)
                        ? role
                        : UserRole.Seller
                };
            }

            return null;
        }
    }
}