using Application.DTOs.CatgoreyDTOs;
using Application.DTOs.MenuItemDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.IService
{
    public interface ICategoryService
    {
        Task<PaginatedResultDto<GetCatgoreyDto>> GetAllAsync(int pageNum , int PageSize);
        Task<GetCatgoreyDto?> GetByIdAsync(int id);
        Task<PaginatedResultDto<GetCatgoreyDto>?> SearchCatgorey(string searchKey ,int pageNum, int PageSize);
        Task<GetCatgoreyDto> CreateAsync(CreateCatgoreyDto dto, IEnumerable<IFormFile>? files);
        Task UpdateAsync(int id, UpdateCategoryDto dto, IEnumerable<IFormFile> files);
        Task DeleteAsync(int id);
    }
}

