using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Distributor.API.Models.DTOs;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;

namespace CozyComfort.Distributor.API.Controllers
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
        public async Task<IActionResult> GetProducts([FromQuery] PagedRequest request)
        {
            var result = await _productService.GetProductsAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("add-from-manufacturer")]
        [Authorize(Policy = "DistributorOnly")]
        public async Task<IActionResult> AddProductFromManufacturer([FromBody] CreateDistributorProductDto dto)
        {
            var result = await _productService.AddProductFromManufacturerAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("check-stock")]
        public async Task<IActionResult> CheckStock([FromBody] DistributorStockCheckRequest request)
        {
            var result = await _productService.CheckStockAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}/update-stock")]
        [Authorize(Policy = "DistributorOnly")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateDistributorStockDto dto)
        {
            var result = await _productService.UpdateStockAsync(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}