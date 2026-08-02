using Domain_Layer.Entities;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System;
using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.TablesDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Business_Layer.Exceptions;

namespace Resturant_Ordering_System.Application.Services
{
    public class TableService : ITableService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly ILogger<TableService> logger;

        public TableService
            (
            IUow uow,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<TableService> logger
            )
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.logger = logger;
        }
        public async Task<GetTablesDto> CreateTableAsync(CreateTablesDto tableDto)
        {
            logger.LogInformation("Creating a new table with TableNumber: {TableNumber}, Capacity:" +
                " {Capacity}, QrCode: {QrCode}, isActive: {isActive}",
            tableDto.TableNumber, tableDto.Capacity, tableDto.QrCode, tableDto.isActive);
            if (tableDto.Capacity <= 0)
            {
                logger.LogWarning("Invalid capacity value: {Capacity}" +
                ". Capacity must be greater than zero.", tableDto.Capacity);
                throw new BadRequestException("Capacity must be greater than zero.");
            }
            var table = mapper.Map<Tables>(tableDto);
           await uow.Tables.CreateAsync(table);
           await uow.SaveChangesAsync();
           logger.LogInformation("Table created successfully with ID: {Id}", table.Id);
           await cacheService.RemoveAsync("GetTables");
           return mapper.Map<GetTablesDto>(table);
        }

        public async Task DeleteTableAsync(int id)
        {
            var table = await uow.Tables.GetByIdAsync(id);
            if (table == null)
            {
                logger.LogWarning("Table With id {id} Not Found", id);
                throw new NotFoundException($"Table with ID {id} not found.");
            }
            await uow.Tables.DeleteAsync(table);
            await uow.SaveChangesAsync();
            logger.LogInformation("Table with ID {id} deleted successfully", id);
            await cacheService.RemoveAsync("GetTables");
        }

        public Task<PaginatedResultDto<GetTablesDto>> FindTablesAsync(string searchKey, int pageNum, int pageSize)
        {
           var cacheKey = $"FindTables_{searchKey}_{pageNum}_{pageSize}";
            var result = cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var query = uow.Tables.Search_Table_With_SearchKey(searchKey);
                    var tables = await uow.Tables.GetAllPaged(pageNum, pageSize, query);
                    return new PaginatedResultDto<GetTablesDto>
                    {
                        Data = mapper.Map<List<GetTablesDto>>(tables.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = tables.TotalCount
                    };
                }
                );
            return result!;
        }

        public async Task<GetTablesDto?> GetTableByIdAsync(int id)
        {
            logger.LogInformation("Getting table with ID {Id}", id);    
            var table = await uow.Tables.GetByIdAsync(id);
            if (table == null)
            {
                logger.LogWarning("Table With id {id} Not Found", id);
                throw new NotFoundException($"Table with ID {id} not found.");
            }
            return mapper.Map<GetTablesDto>(table);
        }

        public async Task<PaginatedResultDto<GetTablesDto>> GetTablesAsync(int pageNum, int pageSize)
        {
            logger.LogInformation(
              "Attempting To get page {pageNum} And size {pageSize}",
              pageNum,
              pageSize);
            var cacheKey = $"GetTables_{pageNum}_{pageSize}";
            var result = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var Tables = await uow.Tables.
                    GetAllPaged(pageNum, pageSize);
                    return new PaginatedResultDto<GetTablesDto>
                    {
                        Data = mapper.Map<List<GetTablesDto>>(Tables.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = Tables.TotalCount
                    };
                }
                );
           return result!;
        }

        public Task<PaginatedResultDto<GetTablesDto>> GetTablesByActiveStatusAsync(bool status, int pageNum, int pageSize)
        {
            var cacheKey = $"GetTablesByActiveStatus_{status}_{pageNum}_{pageSize}";
            var result = cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var query = uow.Tables.GetTablesByActiveStatus(status);
                    var tables = await uow.Tables.GetAllPaged(pageNum, pageSize, query);
                    return new PaginatedResultDto<GetTablesDto>
                    {
                        Data = mapper.Map<List<GetTablesDto>>(tables.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = tables.TotalCount
                    };
                }
                ); 
            return result!;
        }

        public Task<PaginatedResultDto<GetTablesDto>> GetTablesByCapacityAsync(int capacity, int pageNum, int pageSize)
        {
            var cacheKey = $"GetTablesByCapacity_{capacity}_{pageNum}_{pageSize}";
            var result = cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var query = uow.Tables.GetTablesByCapacity(capacity);
                    var tables = await uow.Tables.GetAllPaged(pageNum, pageSize, query);
                    return new PaginatedResultDto<GetTablesDto>
                    {
                        Data = mapper.Map<List<GetTablesDto>>(tables.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = tables.TotalCount
                    };
                }
                );
            return result!;
        }

        public async Task UpdateTableAsync(int id, UpdateTablesDto tableDto)
        {
            logger.LogInformation("Updating table with ID {id} with new values: TableNumber:" +
                " {TableNumber}, Capacity: {Capacity}, QrCode: {QrCode}, isActive: {isActive}",
            id, tableDto.TableNumber, tableDto.Capacity, tableDto.QrCode, tableDto.isActive);
            if (tableDto.Capacity <= 0)
            {
                logger.LogWarning("Invalid capacity value: {Capacity}" +
                ". Capacity must be greater than zero.", tableDto.Capacity);
                throw new BadRequestException("Capacity must be greater than zero.");
            }
            var table = await uow.Tables.GetByIdAsync(id);
            if (table == null)
            {
                logger.LogWarning("Table With id {id} Not Found", id);
                throw new NotFoundException($"Table with ID {id} not found.");
            }
            mapper.Map(tableDto, table);
            await uow.Tables.UpdateAsync(table);
            await uow.SaveChangesAsync();
            logger.LogInformation("Table with ID {id} updated successfully", id);
            await cacheService.RemoveAsync("GetTables");
        }
    }
}
