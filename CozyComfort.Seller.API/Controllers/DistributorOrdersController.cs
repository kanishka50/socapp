using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/distributor-orders")]
    [ApiController]
    [Authorize(Policy = "SellerOnly")]
    public class DistributorOrdersController : ControllerBase
    {
        private readonly ISellerOrderService _orderService;
        private readonly ILogger<DistributorOrdersController> _logger;

        public DistributorOrdersController(
            ISellerOrderService orderService,
            ILogger<DistributorOrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDistributorOrder([FromBody] CreateSellerDistributorOrderDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<bool>.FailureResult("Invalid order data"));
                }

                var result = await _orderService.CreateDistributorOrderAsync(request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor order");
                return BadRequest(ApiResponse<bool>.FailureResult("Error creating distributor order"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistributorOrders([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _orderService.GetDistributorOrdersAsync(request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor orders");
                return BadRequest(ApiResponse<bool>.FailureResult("Error retrieving orders"));
            }
        }

        [HttpPost("accept-notification")]
        [AllowAnonymous] // Allow distributor API to call this
        public async Task<IActionResult> AcceptOrderNotification([FromBody] DistributorOrderAcceptedDto notification)
        {
            try
            {
                var result = await _orderService.UpdateOrderFromDistributorAcceptanceAsync(notification.DistributorOrderNumber);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order acceptance notification");
                return BadRequest(ApiResponse<bool>.FailureResult("Error processing notification"));
            }
        }
    }
}