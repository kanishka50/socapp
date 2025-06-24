using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CozyComfort.Distributor.API.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace CozyComfort.Distributor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DistributorDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(DistributorDbContext context, ILogger<TestController> logger)
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
                        u.CompanyName
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
        /// Reset password for a user (DEVELOPMENT ONLY - REMOVE IN PRODUCTION)
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

                _logger.LogInformation($"Password reset for user: {request.Email}");

                return Ok(new
                {
                    message = "Password reset successful",
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test password verification
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
                    isActive = user.IsActive,
                    passwordValid = passwordValid,
                    role = user.Role.ToString(),
                    message = passwordValid ? "Password is correct" : "Password is incorrect"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing login");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

    public class TestLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}