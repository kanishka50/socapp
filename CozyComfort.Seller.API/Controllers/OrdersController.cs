using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize(Policy = "SellerOnly")]
    public class OrdersController : ControllerBase
    {
        private readonly ISellerOrderService _orderService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            ISellerOrderService orderService,
            ICustomerOrderService customerOrderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _customerOrderService = customerOrderService;
            _logger = logger;
        }

        [HttpGet("combined")]
        public async Task<IActionResult> GetCombinedOrders([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _orderService.GetCombinedOrdersAsync(request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combined orders");
                return BadRequest(ApiResponse<bool>.FailureResult("Error retrieving orders"));
            }
        }
    }
}