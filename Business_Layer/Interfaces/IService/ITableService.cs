using Application.DTOs.TablesDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface ITableService 
    {
        Task<PaginatedResultDto<GetTablesDto>> GetTablesAsync(int pageNum , int pageSize);
        Task<PaginatedResultDto<GetTablesDto>> FindTablesAsync(string searchKey, int pageNum, int pageSize);
        Task <PaginatedResultDto<GetTablesDto>> GetTablesByActiveStatusAsync(bool status, int pageNum, int pageSize);
        Task<PaginatedResultDto<GetTablesDto>> GetTablesByCapacityAsync(int capacity, int pageNum, int pageSize);
        Task<GetTablesDto?> GetTableByIdAsync(int id);
        Task<GetTablesDto> CreateTableAsync(CreateTablesDto tableDto);
        Task UpdateTableAsync(int id, UpdateTablesDto tableDto); 
        Task DeleteTableAsync(int id);
    }
}
