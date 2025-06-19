using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Manufacturer.API.Models.DTOs;
using CozyComfort.Manufacturer.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Manufacturer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [AllowAnonymous] // Allow viewing products without authentication
        public async Task<IActionResult> GetProducts([FromQuery] PagedRequest request)
        {
            var result = await _productService.GetProductsAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Policy = "ManufacturerOnly")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.CreateProductAsync(dto);
            return result.Success
                ? CreatedAtAction(nameof(GetProduct), new { id = result.Data.Id }, result)
                : BadRequest(result);
        }

        [HttpPost("check-stock")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckStock([FromBody] StockCheckRequest request)
        {
            var result = await _productService.CheckStockAsync(request);
            return Ok(result);
        }
    }
}