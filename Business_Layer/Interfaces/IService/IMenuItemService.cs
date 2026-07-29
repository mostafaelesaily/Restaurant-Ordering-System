using Application.DTOs.MenuItemDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.IService
{
    public interface IMenuItemService
    {
        Task<PaginatedResultDto<GetMenuItemDto>> GetAllAsync(int pageNum, int PageSize);
        Task<GetMenuItemDto?> GetByIdAsync(int id);
        Task<PaginatedResultDto<GetMenuItemDto>?> SearchMenuItem(string searchKey, int pageNum, int pageSize);
        Task<GetMenuItemDto> CreateAsync(CreateMenuItemDto dto, IEnumerable<IFormFile>? files);
        Task UpdateAsync(int id, UpdateMenuItemDto dto, IEnumerable<IFormFile> files);
        Task DeleteAsync(int id);
        Task<PaginatedResultDto<GetMenuItemDto>> GetCategoryMenuItemsAsync(int categoryId, int pageNum, int pageSize);

    }
}
