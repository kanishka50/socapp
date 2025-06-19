using Microsoft.AspNetCore.Mvc;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Distributor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDistributorAuthService _authService;

        public AuthController(IDistributorAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}