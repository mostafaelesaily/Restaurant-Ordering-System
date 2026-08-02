using Application.DTOs.FavoriteDTOs;
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            this.favoriteService = favoriteService;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFavorites(int pageNum, int pageSize)
        {
            var result = await favoriteService.GetAllFavoritesAsync(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFavoriteById(int id)
        {
            var result = await favoriteService.GetFavoriteByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchFavorites(string searchKey, int pageNum, int pageSize)
        {
            var result = await favoriteService.SearchFavoritesAsync(searchKey, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFavoritesByCategory(int categoryId, int pageNum, int pageSize)
        {
            var result = await favoriteService.GetFavoritesByCategoryAsync(categoryId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByMenuItemFavorites(int menuItemId, int pageNum, int pageSize)
        {
            var result = await favoriteService.GetFavoritesByMenuItemAsync(menuItemId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddFavorite([FromQuery] int menuItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await favoriteService.AddFavoriteAsync(userId, menuItemId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await favoriteService.DeleteFavoriteAsync(id, userId);
            return NoContent();

        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyFavorites(int pageNum, int pageSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await favoriteService.GetFavoritesByCustomerIdAsync(userId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> IsFavorite([FromQuery] int menuItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await favoriteService.IsFavoriteExistsAsync(userId, menuItemId);
            return Ok(new { isFavorite = result });
        }

        [HttpDelete("[action]")]
        [Authorize]
        public async Task<IActionResult> RemoveFavoriteByMenuItem([FromQuery] int menuItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await favoriteService.RemoveByMenuItemIdAsync(userId, menuItemId);
            return NoContent();
        }

    }
}
