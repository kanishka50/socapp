using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SellerOnly")]
    public class DistributorController : ControllerBase
    {
        private readonly IDistributorApiService _distributorApiService;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(
            IDistributorApiService distributorApiService,
            ILogger<DistributorController> logger)
        {
            _distributorApiService = distributorApiService;
            _logger = logger;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetDistributorProducts([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _distributorApiService.GetDistributorProductsAsync(request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distributor products");
                return BadRequest(ApiResponse<PagedResult<DistributorProductDto>>.FailureResult("Error fetching distributor products"));
            }
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetDistributorProduct(int id)
        {
            try
            {
                var result = await _distributorApiService.GetDistributorProductByIdAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distributor product {ProductId}", id);
                return BadRequest(ApiResponse<DistributorProductDto>.FailureResult("Error fetching distributor product"));
            }
        }
    }
}