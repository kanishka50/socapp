using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;
using System.Threading.Tasks;

namespace CozyComfort.Manufacturer.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<User>> GetUserByEmailAsync(string email);
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}