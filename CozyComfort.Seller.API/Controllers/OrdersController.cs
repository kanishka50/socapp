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

        // ADD THIS MISSING ENDPOINT - This is the key fix
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                // Try to get as customer order first (since that's what the UI expects)
                var customerOrderResult = await _customerOrderService.GetOrderByIdAsync(id);

                if (customerOrderResult.Success)
                {
                    return Ok(customerOrderResult);
                }

                // If not found as customer order, try as distributor order
                var distributorOrderResult = await _orderService.GetDistributorOrderByIdAsync(id);

                if (distributorOrderResult.Success)
                {
                    return Ok(distributorOrderResult);
                }

                // If not found in either, return 404
                return NotFound(ApiResponse<object>.FailureResult("Order not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID {OrderId}", id);
                return BadRequest(ApiResponse<object>.FailureResult("Error retrieving order"));
            }
        }

        // ADD THIS ENDPOINT FOR UPDATING ORDER STATUS
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                // Try to update as customer order first
                var result = await _customerOrderService.UpdateOrderStatusAsync(id, dto.Status);

                if (result.Success)
                {
                    return Ok(result);
                }

                // If customer order update failed, it might be a distributor order
                // But typically status updates are only for customer orders
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                return BadRequest(ApiResponse<bool>.FailureResult("Error updating order status"));
            }
        }
    }

    // DTO for status updates
    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}