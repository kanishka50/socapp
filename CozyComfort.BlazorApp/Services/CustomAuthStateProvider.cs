using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace CozyComfort.BlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CustomAuthStateProvider> _logger;
        private readonly NavigationManager _navigation;
        private bool _isInitialized = false;

        public CustomAuthStateProvider(
            ILocalStorageService localStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<CustomAuthStateProvider> logger,
            NavigationManager navigation)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _navigation = navigation;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // During prerendering, localStorage is not available
                if (!_isInitialized)
                {
                    // Check if we're in a browser context
                    try
                    {
                        // This will throw during prerendering
                        var test = await _localStorage.GetItemAsync<string>("test");
                        _isInitialized = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // We're prerendering, return empty auth state
                        _logger.LogDebug("Prerendering detected, returning anonymous auth state");
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                }

                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogInformation("No auth token found");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Set token for all HTTP clients
                SetAuthorizationHeader(token);

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                _logger.LogInformation($"User authenticated: {user.Identity?.IsAuthenticated}");

                return new AuthenticationState(user);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // This happens during prerendering
                _logger.LogDebug("JavaScript interop not available (prerendering)");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            try
            {
                var authenticatedUser = new ClaimsPrincipal(
                    new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
                var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
                NotifyAuthenticationStateChanged(authState);

                // Set headers after authentication
                SetAuthorizationHeader(token);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user authentication");
            }
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);

            // Clear headers
            ClearAuthorizationHeaders();
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            try
            {
                var payload = jwt.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs != null)
                {
                    _logger.LogInformation($"JWT Claims: {string.Join(", ", keyValuePairs.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

                    // Check for role claim
                    if (keyValuePairs.TryGetValue("role", out object? roles))
                    {
                        if (roles != null)
                        {
                            var rolesString = roles.ToString() ?? "";

                            if (rolesString.Trim().StartsWith("["))
                            {
                                var parsedRoles = JsonSerializer.Deserialize<string[]>(rolesString);
                                if (parsedRoles != null)
                                {
                                    foreach (var parsedRole in parsedRoles)
                                    {
                                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                                        _logger.LogInformation($"Added role claim: {parsedRole}");
                                    }
                                }
                            }
                            else
                            {
                                claims.Add(new Claim(ClaimTypes.Role, rolesString));
                                _logger.LogInformation($"Added role claim: {rolesString}");
                            }
                        }

                        keyValuePairs.Remove("role");
                    }

                    // Add other claims
                    foreach (var kvp in keyValuePairs)
                    {
                        var key = kvp.Key;
                        var value = kvp.Value?.ToString() ?? "";

                        // Map common JWT claims to .NET claim types
                        var claimType = key switch
                        {
                            "sub" => ClaimTypes.NameIdentifier,
                            "nameid" => ClaimTypes.NameIdentifier,
                            "name" => ClaimTypes.Name,
                            "unique_name" => ClaimTypes.Name,
                            "email" => ClaimTypes.Email,
                            _ => key
                        };

                        claims.Add(new Claim(claimType, value));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JWT claims");
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        private void SetAuthorizationHeader(string token)
        {
            try
            {
                var manufacturerClient = _httpClientFactory.CreateClient("ManufacturerAPI");
                var distributorClient = _httpClientFactory.CreateClient("DistributorAPI");
                var sellerClient = _httpClientFactory.CreateClient("SellerAPI");

                manufacturerClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                distributorClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                sellerClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogDebug("Authorization headers set for all API clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting authorization headers");
            }
        }

        private void ClearAuthorizationHeaders()
        {
            try
            {
                var manufacturerClient = _httpClientFactory.CreateClient("ManufacturerAPI");
                var distributorClient = _httpClientFactory.CreateClient("DistributorAPI");
                var sellerClient = _httpClientFactory.CreateClient("SellerAPI");

                manufacturerClient.DefaultRequestHeaders.Authorization = null;
                distributorClient.DefaultRequestHeaders.Authorization = null;
                sellerClient.DefaultRequestHeaders.Authorization = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing authorization headers");
            }
        }
    }
}