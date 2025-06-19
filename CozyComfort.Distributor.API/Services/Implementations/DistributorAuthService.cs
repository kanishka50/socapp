using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CozyComfort.Distributor.API.Data;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class DistributorAuthService : IDistributorAuthService
    {
        private readonly DistributorDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DistributorAuthService> _logger;

        public DistributorAuthService(
            DistributorDbContext context,
            IConfiguration configuration,
            ILogger<DistributorAuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return ApiResponse<TokenDto>.FailureResult("Invalid email or password");
                }

                var token = GenerateJwtToken(user);
                var tokenDto = new TokenDto
                {
                    Token = token,
                    RefreshToken = Guid.NewGuid().ToString(),
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                    Role = user.Role.ToString(),
                    UserName = user.FullName,
                    Email = user.Email
                };

                return ApiResponse<TokenDto>.SuccessResult(tokenDto, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse<TokenDto>.FailureResult($"Login failed: {ex.Message}");
            }
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("CompanyName", user.CompanyName ?? "")
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public async Task<ApiResponse<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null
                ? ApiResponse<User>.SuccessResult(user)
                : ApiResponse<User>.FailureResult("User not found");
        }
    }
}