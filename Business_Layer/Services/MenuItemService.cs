using Application.DTOs.MenuItemDTOs;
using Application.Interfaces.IService;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IFileService fileService;
        private readonly ILogger<MenuItemService> logger;
        private readonly IMapper mapper;
        public MenuItemService(
            IUow uow,
            IFileService fileService,
            ICacheService cacheService,
            ILogger<MenuItemService> logger,
            IMapper mapper)
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.fileService = fileService;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<GetMenuItemDto>> GetAllAsync(int pageNum, int PageSize)
        {
            logger.LogInformation(
                "Attempting To get page {pageNum} And size {pageSize}",
                pageNum,
                PageSize);

            var cacheKey = $"Get_MenuItems_pageNum:{pageNum}_pageSize:{PageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.MenuItems.Query();

                var menuItems = await uow.MenuItems.GetAllPaged(
                    pageNum,
                    PageSize);

                return new PaginatedResultDto<GetMenuItemDto>
                {
                    Data = mapper.Map<List<GetMenuItemDto>>(menuItems.Data),
                    PageNumber = pageNum,
                    PageSize = PageSize,
                    TotalCount = menuItems.TotalCount
                };
            });

            return result!;

        }

        public async Task<GetMenuItemDto?> GetByIdAsync(int id)
        {
            logger.LogInformation("Attemping to get MenuItem with id {id}", id);
            var item = await uow.MenuItems.GetByIdAsync(id);
            if (item == null)
            {
                logger.LogWarning("MenuItem With id {id} Not Found", id);
                throw new NotFoundException("MenuItem Not Found");
            }
            return mapper.Map<GetMenuItemDto>(item);
        }

        public async Task<PaginatedResultDto<GetMenuItemDto>?> SearchMenuItem(string searchKey, int pageNum, int pageSize)
        {
            logger.LogInformation(
                "Attempting To Search MenuItems : page {pageNum} And size {pageSize}",
                pageNum,
                pageSize);

            var cacheKey = $"Search_MenuItem_{searchKey}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.MenuItems.Search_MenuItem_With_Name_Desc(searchKey);

                var menuItems = await uow.MenuItems.GetAllPaged(
                    pageNum,
                    pageSize,
                    query);

                return new PaginatedResultDto<GetMenuItemDto>
                {
                    Data = mapper.Map<List<GetMenuItemDto>>(menuItems.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = menuItems.TotalCount
                };
            });

            return result!;

        }

        public async Task<GetMenuItemDto> CreateAsync(CreateMenuItemDto dto, IEnumerable<IFormFile>? files)
        {
            logger.LogInformation("Attemping To Create MenuItem");
            var uploadedFiles = new List<string>();
            await using var Transaction = await uow.BeginTransactionAsync();
            try
            {
                var menuItem = mapper.Map<MenuItems>(dto);
                await uow.MenuItems.CreateAsync(menuItem);
                await uow.SaveChangesAsync();
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var filePath = await fileService.UploadFileAsync(file, "MenuItems");
                        uploadedFiles.Add(filePath);
                        var menuFile = new Files
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            FileType = file.ContentType,
                            menuItemId = menuItem.id,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await uow.Files.CreateAsync(menuFile);
                    }

                    await uow.SaveChangesAsync();
                }

                await Transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_MenuItems");
                await cacheService.RemoveAsync("Search_MenuItem");
                return mapper.Map<GetMenuItemDto>(menuItem);
            }
            catch
            {
                foreach (var file in uploadedFiles)
                {
                    await fileService.DeleteFileAsync(file);
                }

                await Transaction.RollbackAsync();

                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateMenuItemDto dto, IEnumerable<IFormFile>? files)
        {
            logger.LogInformation("Attempting to update MenuItem with id {id}", id);

            await using var transaction = await uow.BeginTransactionAsync();

            var uploadedFiles = new List<string>();
            var oldFiles = new List<Files>();

            try
            {
                var menuItem = await uow.MenuItems.GetByIdAsync(id);

                if (menuItem == null)
                {
                    logger.LogWarning("MenuItem With id {id} Not Found", id);
                    throw new NotFoundException("MenuItem Not Found");
                }

                mapper.Map(dto, menuItem);
                Console.WriteLine($"DTO CategoryId = {dto.categoryId}");
                Console.WriteLine($"MenuItem CategoryId = {menuItem.categoryId}");

                if (files != null && files.Any())
                {
                    oldFiles = await uow.Files.Query()
                        .Where(f => f.menuItemId == id)
                        .ToListAsync();

                    foreach (var file in files)
                    {
                        var filePath = await fileService.UploadFileAsync(file, "MenuItems");
                        uploadedFiles.Add(filePath);

                        await uow.Files.CreateAsync(new Files
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            FileType = file.ContentType,
                            menuItemId = menuItem.id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    foreach (var oldFile in oldFiles)
                    {
                        await fileService.DeleteFileAsync(oldFile.FilePath);
                        await uow.Files.DeleteAsync(oldFile);
                    }
                }
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();

                await cacheService.RemoveAsync("Get_MenuItems");
                await cacheService.RemoveAsync("Search_MenuItem");
            }
            catch
            {
                await transaction.RollbackAsync();

                foreach (var filePath in uploadedFiles)
                {
                    await fileService.DeleteFileAsync(filePath);
                }

                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            logger.LogInformation("Attempting to delete MenuItem with id {id}", id);

            await using var transaction = await uow.BeginTransactionAsync();

            try
            {
                var item = await uow.MenuItems.GetByIdAsync(id);

                if (item == null)
                {
                    logger.LogWarning("MenuItem with id {id} Not Found", id);
                    throw new NotFoundException("MenuItem Not Found");
                }

                var files = await uow.Files.Query()
                    .Where(f => f.menuItemId == item.id)
                    .ToListAsync();

                foreach (var file in files)
                {
                    await uow.Files.DeleteAsync(file);
                }

                await uow.MenuItems.DeleteAsync(item);

                await uow.SaveChangesAsync();
                await transaction.CommitAsync();

                foreach (var file in files)
                {
                    await fileService.DeleteFileAsync(file.FilePath);
                }

                await cacheService.RemoveAsync("Get_MenuItems");
                await cacheService.RemoveAsync("Search_MenuItem");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<PaginatedResultDto<GetMenuItemDto>> GetCategoryMenuItemsAsync(
        int categoryId,
        int pageNum,
        int pageSize)
        {
            var category = await uow.Categories.GetByIdAsync(categoryId);

            if (category == null)
            {
                throw new NotFoundException("Category Not Found");
            }

            var query = uow.MenuItems.GetCategoreyMenuItems(categoryId);

            var menuItems = await uow.MenuItems.GetAllPaged(pageNum, pageSize, query);

            
            if (menuItems.TotalCount == 0)
            {
                throw new NotFoundException("No MenuItems For This Category");
            }

            return new PaginatedResultDto<GetMenuItemDto>
            {
                Data = mapper.Map<List<GetMenuItemDto>>(menuItems.Data),
                PageNumber = pageNum,
                PageSize = pageSize,
                TotalCount = menuItems.TotalCount,
            };
        }
    }
}
