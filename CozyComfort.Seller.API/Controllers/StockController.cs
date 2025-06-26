using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<StockController> _logger;

        public StockController(IInventoryService inventoryService, ILogger<StockController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpPost("update-bulk")]
        [AllowAnonymous] // Allow distributor API to call this
        public async Task<IActionResult> UpdateStockBulk([FromBody] UpdateSellerStockBulkDto dto)
        {
            // Validate API key or use service-to-service authentication
            var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
            if (apiKey != "DistributorAPIKey123") // Configure this properly
            {
                return Unauthorized();
            }

            var result = await _inventoryService.UpdateStockBulkAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}