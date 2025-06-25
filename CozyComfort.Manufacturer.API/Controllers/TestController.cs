using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CozyComfort.Manufacturer.API.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Manufacturer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ManufacturerDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(ManufacturerDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to check database connectivity and users
        /// </summary>
        [HttpGet("check-users")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.Role,
                        u.IsActive,
                        u.CompanyName,
                        u.CreatedAt,
                        u.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Database connection successful",
                    userCount = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking users");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reset password for a manufacturer user (DEVELOPMENT ONLY - REMOVE IN PRODUCTION)
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Email and password are required");
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return NotFound($"User with email {request.Email} not found");
                }

                // Hash the new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = "PasswordReset";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset for manufacturer user: {request.Email}");

                return Ok(new
                {
                    message = "Password reset successful",
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString(),
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for manufacturer");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test password verification for manufacturer
        /// </summary>
        [HttpPost("test-login")]
        [AllowAnonymous]
        public async Task<IActionResult> TestLogin([FromBody] TestLoginRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return Ok(new { found = false, message = "User not found" });
                }

                var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                return Ok(new
                {
                    found = true,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    isActive = user.IsActive,
                    passwordValid = passwordValid,
                    role = user.Role.ToString(),
                    companyName = user.CompanyName,
                    message = passwordValid ? "Password is correct" : "Password is incorrect"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing manufacturer login");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new manufacturer user for testing
        /// </summary>
        [HttpPost("create-manufacturer")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateManufacturer([FromBody] CreateManufacturerRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest($"User with email {request.Email} already exists");
                }

                var user = new CozyComfort.Shared.Models.User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CompanyName = request.CompanyName ?? "CozyComfort Manufacturing",
                    Role = UserRole.Manufacturer,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestController",
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created manufacturer user: {request.Email}");

                return Ok(new
                {
                    message = "Manufacturer user created successfully",
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString(),
                    companyName = user.CompanyName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manufacturer user");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activate or deactivate a user
        /// </summary>
        [HttpPost("toggle-user-status")]
        [AllowAnonymous]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleUserStatusRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return NotFound($"User with email {request.Email} not found");
                }

                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = "TestController";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"User {(request.IsActive ? "activated" : "deactivated")} successfully",
                    email = user.Email,
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get database information
        /// </summary>
        [HttpGet("db-info")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var manufacturerUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Manufacturer);
                var products = await _context.Products.CountAsync();

                return Ok(new
                {
                    message = "Database information",
                    totalUsers,
                    activeUsers,
                    manufacturerUsers,
                    totalProducts = products,
                    databaseConnected = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class TestLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateManufacturerRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
    }

    public class ToggleUserStatusRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}