using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CozyComfort.Manufacturer.API.Data;
using CozyComfort.Manufacturer.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;

namespace CozyComfort.Manufacturer.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ManufacturerDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ManufacturerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null)
                {
                    return ApiResponse<TokenDto>.FailureResult("Invalid email or password");
                }

                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return ApiResponse<TokenDto>.FailureResult("Invalid email or password");
                }

                var token = GenerateJwtToken(user);
                var tokenDto = new TokenDto
                {
                    Token = token,
                    RefreshToken = Guid.NewGuid().ToString(), // Simplified for now
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                    Role = user.Role.ToString(),
                    UserName = user.FullName,
                    Email = user.Email
                };

                return ApiResponse<TokenDto>.SuccessResult(tokenDto, "Login successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<TokenDto>.FailureResult($"Login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null
                ? ApiResponse<User>.SuccessResult(user)
                : ApiResponse<User>.FailureResult("User not found");
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}