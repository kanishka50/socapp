
using CozyComfort.Manufacturer.API.Services.Interfaces;
using CozyComfort.Shared.DTOs.Manufacturer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.Manufacturer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetOrdersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("from-distributor")]
        [AllowAnonymous] // Allow distributors to create orders
        public async Task<IActionResult> CreateOrderFromDistributor([FromBody] CreateManufacturerOrderFromDistributorDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _orderService.CreateOrderFromDistributorAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "ManufacturerOnly")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _orderService.UpdateOrderStatusAsync(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var result = await _orderService.GetInventoryAsync();
            return Ok(result);
        }
    }
}