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

        public CustomAuthStateProvider(
            ILocalStorageService localStorage,
            IHttpClientFactory httpClientFactory)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Set token for all HTTP clients
                SetAuthorizationHeader(token);

                return new AuthenticationState(new ClaimsPrincipal(
                    new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
            }
            catch (Exception ex)
            {
                // Return anonymous user on error
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
                // Handle error silently or throw based on requirements
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

            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles))
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp =>
                new Claim(kvp.Key, kvp.Value.ToString())));

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
                // Handle error silently
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
                // Handle error silently
            }
        }
    }
}