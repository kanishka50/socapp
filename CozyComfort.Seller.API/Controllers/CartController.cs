using Microsoft.AspNetCore.Mvc;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetCart(string sessionId)
        {
            var result = await _cartService.GetCartAsync(sessionId);
            return Ok(result);
        }

        [HttpPost("{sessionId}/add")]
        public async Task<IActionResult> AddToCart(string sessionId, [FromBody] AddToCartDto dto)
        {
            var result = await _cartService.AddToCartAsync(sessionId, dto);
            return Ok(result);
        }

        [HttpPut("{sessionId}/update")]
        public async Task<IActionResult> UpdateCartItem(string sessionId, [FromBody] dynamic dto)
        {
            int productId = dto.ProductId;
            int quantity = dto.Quantity;
            var result = await _cartService.UpdateCartItemAsync(sessionId, productId, quantity);
            return Ok(result);
        }

        [HttpDelete("{sessionId}/remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string sessionId, int productId)
        {
            var result = await _cartService.RemoveFromCartAsync(sessionId, productId);
            return Ok(result);
        }
    }
}