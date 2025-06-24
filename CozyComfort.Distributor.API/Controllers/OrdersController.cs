using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using CozyComfort.Distributor.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Distributor.API.Controllers
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

        [HttpPost("create-manufacturer-order")]
        [Authorize(Policy = "DistributorOnly")]
        public async Task<IActionResult> CreateManufacturerOrder([FromBody] CreateManufacturerOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.CreateManufacturerOrderAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("process-seller-order")]
        [Authorize(Policy = "SystemOrDistributor")]
        public async Task<IActionResult> ProcessSellerOrder([FromBody] ProcessSellerOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.ProcessSellerOrderAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/update-status")]
        [Authorize(Policy = "DistributorOnly")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}