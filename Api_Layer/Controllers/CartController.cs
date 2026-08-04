using Resturant_Ordering_System.Application.DTOs.CartDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService cartService;

        public CartController(ICartService cartService)
        {
            this.cartService = cartService;
        }

        [HttpGet("[action]")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCarts(int pageNumber, int pageSize)
        {
            var result = await cartService.GetAllCartsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyCart()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await cartService.GetCartAsync(customerId);
            return Ok(result);
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await cartService.AddToCartAsync(customerId, dto);
            return Ok(new { message = "Item added to cart successfully" });
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartDto dto)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await cartService.UpdateCartItemAsync(customerId, cartItemId, dto);
            return Ok(new { message = "Cart item updated successfully" });
        }

        [HttpDelete("[action]")]
        [Authorize]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await cartService.RemoveCartItemAsync(customerId, cartItemId);
            return NoContent();
        }

        [HttpDelete("[action]")]
        [Authorize]
        public async Task<IActionResult> ClearCart()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await cartService.ClearCartAsync(customerId);
            return NoContent();
        }
    }
}
