using Application.DTOs.MenuItemDTOs;
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemService menuItemService;

        public MenuItemController(IMenuItemService menuItemService)
        {
            this.menuItemService = menuItemService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllMenuItems(int pageNum, int pageSize)
        {
            var result = await menuItemService.GetAllAsync(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await menuItemService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SearchMenuItems(string searchKey, int pageNum, int pageSize)
        {
            var result = await menuItemService.SearchMenuItem(searchKey, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateMenuItem([FromForm] CreateMenuItemDto dto, [FromForm] IFormFileCollection? files)
        {
            var result = await menuItemService.CreateAsync(dto, files);
            return CreatedAtAction(nameof(GetById), new { id = result.id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromForm] UpdateMenuItemDto dto, [FromForm] IFormFileCollection? files)
        {
            await menuItemService.UpdateAsync(id, dto, files);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            await menuItemService.DeleteAsync(id);
            return NoContent();
        }
    }
}
