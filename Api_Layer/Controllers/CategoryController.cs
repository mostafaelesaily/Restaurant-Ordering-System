using Application.DTOs.CatgoreyDTOs;
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;
        private readonly IMenuItemService menuItemService;

       public CategoryController(ICategoryService categoryService , IMenuItemService menuItemService)
        {
            this.categoryService = categoryService;
            this.menuItemService = menuItemService;
        }

       [HttpGet("[action]")]
        public async Task<IActionResult> GetAllCategory(int pageNum, int pageSize)
        {
            var result = await categoryService.GetAllAsync(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await categoryService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SearchCatgorey(string searchKey, int pageNum, int pageSize)
        {
            var result = await categoryService.SearchCatgorey(searchKey, pageNum, pageSize);
            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetCategoreyMenuItems(int categoreyId,int  pageNum, int pageSize)
        {
            var result = await menuItemService.GetCategoryMenuItemsAsync(categoreyId,pageNum,pageSize);
            return Ok(result);
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromForm] CreateCatgoreyDto dto, [FromForm] IFormFileCollection? files)
        {
            var result = await categoryService.CreateAsync(dto, files);
            return CreatedAtAction(nameof(GetById), new { id = result.id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto dto, [FromForm] IFormFileCollection? files)
        {
            await categoryService.UpdateAsync(id, dto, files);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await categoryService.DeleteAsync(id);
            return NoContent();
        }

    }
}
