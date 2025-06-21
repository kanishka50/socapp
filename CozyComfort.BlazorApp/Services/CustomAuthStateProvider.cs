using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;

namespace CozyComfort.BlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(
            ILocalStorageService localStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<CustomAuthStateProvider> logger)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
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
                _logger.LogInformation($"User claims: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");

                return new AuthenticationState(user);
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
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                // Log all claims for debugging
                _logger.LogInformation($"JWT Claims: {string.Join(", ", keyValuePairs.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

                // Check for role claim with different possible keys
                var roleKeys = new[] { "role", "Role", "roles", "Roles", ClaimTypes.Role,
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };

                foreach (var roleKey in roleKeys)
                {
                    if (keyValuePairs.TryGetValue(roleKey, out object? roles))
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

                        keyValuePairs.Remove(roleKey);
                        break;
                    }
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
                        "name" => ClaimTypes.Name,
                        "email" => ClaimTypes.Email,
                        _ => key
                    };

                    claims.Add(new Claim(claimType, value));
                }
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