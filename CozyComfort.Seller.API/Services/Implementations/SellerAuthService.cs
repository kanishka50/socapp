using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CozyComfort.Seller.API.Data;
//using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class SellerAuthService : ISellerAuthService
    {
        private readonly SellerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SellerAuthService> _logger;

        public SellerAuthService(SellerDbContext context, IConfiguration configuration, ILogger<SellerAuthService> logger)
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
                return ApiResponse<TokenDto>.FailureResult("Login failed");
            }
        }

        public async Task<ApiResponse<User>> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            try
            {
                // Check if user exists
                var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
                if (existingUser)
                {
                    return ApiResponse<User>.FailureResult("User with this email already exists");
                }

                var user = new User
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = UserRole.Seller, // Or create a Customer role
                    Phone = dto.Phone,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return ApiResponse<User>.SuccessResult(user, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return ApiResponse<User>.FailureResult("Registration failed");
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
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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