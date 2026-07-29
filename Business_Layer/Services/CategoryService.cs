using Application.DTOs.CatgoreyDTOs;
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
using System.Text;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IFileService fileService;
        private readonly ILogger<CategoryService> logger;
        private readonly IMapper mapper;
        public CategoryService
            (
            IUow uow,
            IFileService fileService,
            ICacheService cacheService,
            ILogger<CategoryService> logger,
            IMapper mapper
            )
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.fileService = fileService;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<GetCatgoreyDto>> GetAllAsync(int pageNum, int PageSize)
        {
            logger.LogInformation(
                "Attempting To get page {pageNum} And size {pageSize}",
                pageNum,
                PageSize);

            var cacheKey = $"Get_Categorey_pageNum:{pageNum}_pageSize:{PageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Categories.Query();

                var categories = await uow.Categories.GetAllPaged(
                    pageNum,
                    PageSize);

                return new PaginatedResultDto<GetCatgoreyDto>
                {
                    Data = mapper.Map<List<GetCatgoreyDto>>(categories.Data),
                    PageNumber = pageNum,
                    PageSize = PageSize,
                    TotalCount = categories.TotalCount
                };
            });

            return result!;
        }

        public async Task<GetCatgoreyDto?> GetByIdAsync(int id)
        {
            logger.LogInformation("Attemping to get Catgorey with id {id}", id);
            var Catgorey = await uow.Categories.GetByIdAsync(id);
            if (Catgorey == null)
            {
                logger.LogWarning("Catgorey With id {id} Not Found", id);
                throw new NotFoundException("Catgorey Not Found");
            }
            return mapper.Map<GetCatgoreyDto>(Catgorey);
        }

        public async Task<PaginatedResultDto<GetCatgoreyDto>?> SearchCatgorey(
        string searchKey,
        int pageNum,
        int pageSize)
        {
            logger.LogInformation(
                "Attempting To Search Category : page {pageNum} And size {pageSize}",
                pageNum,
                pageSize);

            var cacheKey = $"Search_Category_{searchKey}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Categories.Search_Catgorey_With_Name_Desc(searchKey);

                var categories = await uow.Categories.GetAllPaged(
                    pageNum,
                    pageSize,
                    query);

                return new PaginatedResultDto<GetCatgoreyDto>
                {
                    Data = mapper.Map<List<GetCatgoreyDto>>(categories.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = categories.TotalCount
                };
            });

            return result!;
        } 
        public async Task<GetCatgoreyDto> CreateAsync(CreateCatgoreyDto dto, IEnumerable<IFormFile>? files)
        {
            logger.LogInformation("Attemping To Create Catgorey");
            var uploadedFiles = new List<string>();
            await using var Transaction = await uow.BeginTransactionAsync();
            try
            {
                var Catgorey = mapper.Map<Categories>(dto);
                await uow.Categories.CreateAsync(Catgorey);
                await uow.SaveChangesAsync();
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var filePath = await fileService.UploadFileAsync(file, "Categories");
                        uploadedFiles.Add(filePath);
                        var catgoreyFile = new Files
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            FileType = file.ContentType,
                            categoryId = Catgorey.id,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await uow.Files.CreateAsync(catgoreyFile);
                    }

                    await uow.SaveChangesAsync();
                }

                await Transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Categorey");
                await cacheService.RemoveAsync("Search_Category");
                return mapper.Map<GetCatgoreyDto>(Catgorey);
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

        public async Task UpdateAsync(int id, UpdateCategoryDto dto, IEnumerable<IFormFile>? files)
        {
            logger.LogInformation("Attempting to update Category with id {id}", id);

            await using var transaction = await uow.BeginTransactionAsync();

            var uploadedFiles = new List<string>();
            var oldFiles = new List<Files>();

            try
            {
                var category = await uow.Categories.GetByIdAsync(id);

                if (category == null)
                {
                    logger.LogWarning("Category With id {id} Not Found", id);
                    throw new NotFoundException("Category Not Found");
                }

                mapper.Map(dto, category);

                if (files != null && files.Any())
                {
                    oldFiles = await uow.Files.Query()
                        .Where(f => f.categoryId == id)
                        .ToListAsync();

                    foreach (var file in files)
                    {
                        var filePath = await fileService.UploadFileAsync(file, "Categories");
                        uploadedFiles.Add(filePath);

                        await uow.Files.CreateAsync(new Files
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            FileType = file.ContentType,
                            categoryId = category.id,
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

                await cacheService.RemoveAsync("Get_Categorey");
                await cacheService.RemoveAsync("Search_Category");
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
            logger.LogInformation("Attempting to delete Category with id {id}", id);

            await using var transaction = await uow.BeginTransactionAsync();

            try
            {
                var category = await uow.Categories.GetByIdAsync(id);

                if (category == null)
                {
                    logger.LogWarning("Category with id {id} Not Found", id);
                    throw new NotFoundException("Category Not Found");
                }

                var files = await uow.Files.Query()
                    .Where(f => f.categoryId == category.id)
                    .ToListAsync();

                foreach (var file in files)
                {
                    await uow.Files.DeleteAsync(file);
                }

                await uow.Categories.DeleteAsync(category);

                await uow.SaveChangesAsync();
                await transaction.CommitAsync();

                foreach (var file in files)
                {
                    await fileService.DeleteFileAsync(file.FilePath);
                }

                await cacheService.RemoveAsync("Get_Categorey");
                await cacheService.RemoveAsync("Search_Category");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        

    }
}
