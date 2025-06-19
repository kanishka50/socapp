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

                var result = JsonSerializer.Deserialize<ApiResponse<TokenDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result.Success && result.Data != null)
                {
                    await _localStorage.SetItemAsync("authToken", result.Data.Token);
                    await _localStorage.SetItemAsync("userRole", result.Data.Role);
                    await _localStorage.SetItemAsync("apiEndpoint", apiEndpoint);

                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
                }

                return result;
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

            if (user.Identity.IsAuthenticated)
            {
                return new User
                {
                    Email = user.FindFirst(ClaimTypes.Email)?.Value,
                    FirstName = user.FindFirst(ClaimTypes.Name)?.Value?.Split(' ')[0],
                    LastName = user.FindFirst(ClaimTypes.Name)?.Value?.Split(' ').LastOrDefault(),
                    Role = Enum.Parse<UserRole>(user.FindFirst(ClaimTypes.Role)?.Value ?? "Seller")
                };
            }

            return null;
        }
    }
}