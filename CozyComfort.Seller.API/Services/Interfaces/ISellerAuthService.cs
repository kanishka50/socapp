using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;
//using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface ISellerAuthService
    {
        Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<User>> RegisterCustomerAsync(RegisterCustomerDto dto);
        Task<ApiResponse<User>> GetUserByEmailAsync(string email);
        string GenerateJwtToken(User user);
        bool VerifyPassword(string password, string hash);
    }
}
