using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;

namespace CozyComfort.Distributor.API.Services.Interfaces
{
    public interface IDistributorAuthService
    {
        Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<User>> GetUserByEmailAsync(string email);
        string GenerateJwtToken(User user);
        bool VerifyPassword(string password, string hash);
    }
}