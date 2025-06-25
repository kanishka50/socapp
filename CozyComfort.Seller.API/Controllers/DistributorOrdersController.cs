using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs.Distributor;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SellerOnly")]
    public class DistributorOrdersController : ControllerBase
    {
        private readonly IDistributorApiService _distributorApiService;
        private readonly ILogger<DistributorOrdersController> _logger;

        public DistributorOrdersController(
            IDistributorApiService distributorApiService,
            ILogger<DistributorOrdersController> logger)
        {
            _distributorApiService = distributorApiService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDistributorOrder([FromBody] CreateDistributorOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<bool>.FailureResult("Invalid order data"));
                }

                if (!request.Items.Any())
                {
                    return BadRequest(ApiResponse<bool>.FailureResult("Order must contain at least one item"));
                }

                // Convert request to distributor API format
                var distributorItems = request.Items.Select(item => new DistributorOrderItem
                {
                    DistributorProductId = item.DistributorProductId,
                    Quantity = item.Quantity,
                    RequestedPrice = item.RequestedPrice
                }).ToList();

                // Create order with distributor
                var result = await _distributorApiService.CreateDistributorOrderAsync(distributorItems);

                if (result.Success)
                {
                    _logger.LogInformation($"Distributor order created successfully for {distributorItems.Count} items. Reference: {request.OrderReference}");
                }

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return BadRequest(ApiResponse<bool>.FailureResult("Error creating distributor order"));
            }
        }
    }
}