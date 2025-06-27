using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;

// Note: For the RZ10012 error about 'RedirectToLogin' in your Blazor component,
// you need to add @using directive for the namespace containing RedirectToLogin component
// or use the fully qualified name in your .razor file

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/customer-orders")]
    [ApiController]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ILogger<CustomerOrdersController> _logger;

        public CustomerOrdersController(
            ICustomerOrderService customerOrderService,
            ILogger<CustomerOrdersController> logger)
        {
            _customerOrderService = customerOrderService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new customer order from the shopping cart
        /// This endpoint is accessible without authentication for public customers
        /// </summary>
        [HttpPost("create")]
        [AllowAnonymous] // Allow public customers to create orders
        public async Task<IActionResult> CreateCustomerOrder([FromBody] CreateCustomerOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<CustomerOrderDto>.FailureResult("Invalid order data"));
                }

                _logger.LogInformation("Creating customer order for session: {SessionId}", dto.SessionId);

                var result = await _customerOrderService.CreateOrderAsync(dto);

                if (result.Success)
                {
                    _logger.LogInformation("Customer order created successfully: {OrderNumber}", result.Data?.OrderNumber);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to create customer order: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer order");
                return BadRequest(ApiResponse<CustomerOrderDto>.FailureResult("Error creating order"));
            }
        }

        /// <summary>
        /// Get customer orders by email
        /// This requires authentication
        /// </summary>
        [HttpGet("by-email/{email}")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> GetCustomerOrdersByEmail(string email)
        {
            try
            {
                var result = await _customerOrderService.GetCustomerOrdersAsync(email);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer orders for email: {Email}", email);
                return BadRequest(ApiResponse<List<CustomerOrderDto>>.FailureResult("Error retrieving orders"));
            }
        }

        /// <summary>
        /// Get a specific customer order by ID
        /// This requires authentication
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> GetCustomerOrderById(int id)
        {
            try
            {
                var result = await _customerOrderService.GetOrderByIdAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer order by ID: {OrderId}", id);
                return BadRequest(ApiResponse<CustomerOrderDto>.FailureResult("Error retrieving order"));
            }
        }

        /// <summary>
        /// Update customer order status
        /// This requires authentication
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> UpdateCustomerOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                var result = await _customerOrderService.UpdateOrderStatusAsync(id, dto.Status);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer order status for order {OrderId}", id);
                return BadRequest(ApiResponse<bool>.FailureResult("Error updating order status"));
            }
        }
    }

}