using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Application.DTOs.TablesDTOs;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController : ControllerBase
    {
        private readonly ITableService tableService;
        public TablesController(ITableService tableService) 
        { 
            this.tableService = tableService;
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllTables(int pageNum, int pageSize)
        {
            var tables = await tableService.GetTablesAsync(pageNum, pageSize);
            return Ok(tables);
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTableById(int id)
        {
            var table = await tableService.GetTableByIdAsync(id);
            return Ok(table);
        }
        
        [Authorize(Roles = "Admin,Manager")]    
        [HttpGet("[action]")]
        public async Task<IActionResult> SearchTables(string searchKey, int pageNum, int pageSize)
        {
            var result = await tableService.FindTablesAsync(searchKey, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTablesByActiveStatus(bool isActive, int pageNum, int pageSize)
        {
            var result = await tableService.GetTablesByActiveStatusAsync(isActive, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTablesByCapacity(int capacity, int pageNum, int pageSize)
        {
            var result = await tableService.GetTablesByCapacityAsync(capacity, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateTable([FromBody] CreateTablesDto dto)
        {
            var created = await tableService.CreateTableAsync(dto);
            return CreatedAtAction(nameof(GetTableById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTablesDto dto)
        {
            await tableService.UpdateTableAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            await tableService.DeleteTableAsync(id);
            return NoContent();
        }

    }
}
