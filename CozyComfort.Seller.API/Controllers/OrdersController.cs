using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
//using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICustomerOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ICustomerOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateCustomerOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.CreateOrderAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("customer/{email}")]
        [Authorize]
        public async Task<IActionResult> GetCustomerOrders(string email)
        {
            var result = await _orderService.GetCustomerOrdersAsync(email);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> GetAllOrders([FromQuery] PagedRequest request)
        {
            var result = await _orderService.GetOrdersAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}