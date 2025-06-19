using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;

namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<TokenDto>> LoginAsync(string apiEndpoint, LoginDto loginDto);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync();
        Task<User> GetCurrentUserAsync();
    }
}