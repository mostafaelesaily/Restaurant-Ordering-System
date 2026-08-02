using Application.DTOs.FavoriteDTOs;
using Application.Interfaces.IService;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly ILogger<FavoriteService> logger;

        public FavoriteService(IUow uow, ICacheService cacheService, IMapper mapper, ILogger<FavoriteService> logger)
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<PaginatedResultDto<GetFavoriteDto>> GetAllFavoritesAsync(int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get favorites page {pageNum} size {pageSize}", pageNum, pageSize);
            var cacheKey = $"Get_Favorites_pageNum:{pageNum}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var favorites = await uow.FavoriteRepo.GetAllPaged(pageNum, pageSize);
                return new PaginatedResultDto<GetFavoriteDto>
                {
                    Data = mapper.Map<List<GetFavoriteDto>>(favorites.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = favorites.TotalCount
                };
            });

            return result!;
        }

        public async Task<GetFavoriteDto?> GetFavoriteByIdAsync(int id)
        {
            logger.LogInformation("Attempting to get favorite with id {id}", id);
            var favorite = await uow.FavoriteRepo.GetByIdAsync(id);
            if (favorite == null)
            {
                logger.LogWarning("Favorite with id {id} not found", id);
                throw new NotFoundException("Favorite not found");
            }
            return mapper.Map<GetFavoriteDto>(favorite);
        }

        public async Task<GetFavoriteDto> AddFavoriteAsync(string customerId, int menuItemId)
        {
            logger.LogInformation("Attempting to add favorite for customer {customerId} and menu item {menuItemId}", 
                customerId, menuItemId);

            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var user = await uow.AppUserRepo.GetByIdAsync(customerId);
                if (user == null)
                {
                    logger.LogInformation("Customer with id {customerId} does not exist", customerId);
                    throw new NotFoundException("Customer not found");
                }
                var menuItem = await uow.MenuItems.GetByIdAsync(menuItemId);
                if (menuItem == null)
                {
                    logger.LogInformation("Menu item with id {menuItemId} does not exist", menuItemId);
                    throw new NotFoundException("Menu item not found");
                }
                var existingFavorite = await uow.FavoriteRepo.GetFavoriteByCustomerAndMenuItemAsync(customerId, menuItemId);
                if (existingFavorite != null)
                {
                    logger.LogInformation("Favorite already exists for customer {customerId} and menu item {menuItemId}",
                        customerId, menuItemId);
                    throw new BadRequestException("Favorite already exists");
                }
                var favorite = new Favorite
                {
                    CustomerId = customerId,
                    MenuItemId = menuItemId
                };
                await uow.FavoriteRepo.CreateAsync(favorite);
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Favorites");
                logger.LogInformation("Favorite added successfully with ID {id}", favorite.Id);
                return mapper.Map<GetFavoriteDto>(favorite);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task DeleteFavoriteAsync(int id , string userId)
        {
            logger.LogInformation("Attempting to delete favorite with id {id}", id);
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var favorite = await uow.FavoriteRepo.GetFavoriteByIdAndCustomerIdAsync(id, userId);
                if (favorite == null)
                {
                    logger.LogWarning("Favorite with id {id} not found", id);
                    throw new NotFoundException("Favorite not found");
                }
                await uow.FavoriteRepo.DeleteAsync(favorite);
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Favorites");
                logger.LogInformation("Favorite with ID {id} deleted successfully", id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaginatedResultDto<GetFavoriteDto>> SearchFavoritesAsync(string searchKey, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to search favorites with key {searchKey} page {pageNum} size {pageSize}", 
                searchKey, pageNum, pageSize);
            var cacheKey = $"Search_Favorite_{searchKey}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.FavoriteRepo.SearchFavoritesAsync(searchKey);
                var favorites = await uow.FavoriteRepo.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<GetFavoriteDto>
                {
                    Data = mapper.Map<List<GetFavoriteDto>>(favorites.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = favorites.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<GetFavoriteDto>> GetFavoritesByCategoryAsync(int categoryId, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get favorites by category {categoryId} page {pageNum} size {pageSize}", 
                categoryId, pageNum, pageSize);
            var cacheKey = $"Favorites_Category_{categoryId}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.FavoriteRepo.GetFavoritesByCategoryAsync(categoryId);
                var favorites = await uow.FavoriteRepo.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<GetFavoriteDto>
                {
                    Data = mapper.Map<List<GetFavoriteDto>>(favorites.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = favorites.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<GetFavoriteDto>> GetFavoritesByMenuItemAsync(int menuItemId, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get favorites by menu item {menuItemId} page {pageNum} size {pageSize}", 
                menuItemId, pageNum, pageSize);
            var cacheKey = $"Favorites_MenuItem_{menuItemId}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.FavoriteRepo.GetFavoritesByMenuItemsAsync(menuItemId);
                var favorites = await uow.FavoriteRepo.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<GetFavoriteDto>
                {
                    Data = mapper.Map<List<GetFavoriteDto>>(favorites.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = favorites.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<GetFavoriteDto>> GetFavoritesByCustomerIdAsync(string customerId, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get favorites by customer {customerId} page {pageNum} size {pageSize}",
                customerId, pageNum, pageSize);
            var cacheKey = $"Favorites_Customer_{customerId}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.FavoriteRepo.GetFavoritesByCustomerIdAsync(customerId);
                var favorites = await uow.FavoriteRepo.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<GetFavoriteDto>
                {
                    Data = mapper.Map<List<GetFavoriteDto>>(favorites.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = favorites.TotalCount
                };
            });

            return result!;
        }

        public async Task<bool> IsFavoriteExistsAsync(string customerId, int menuItemId)
        {
            logger.LogInformation("Checking if favorite exists for customer {customerId} and menu item {menuItemId}",
                customerId, menuItemId);
            var favorite = await uow.FavoriteRepo.GetFavoriteByCustomerAndMenuItemAsync(customerId, menuItemId);
            return favorite != null;
        }
        public async Task RemoveByMenuItemIdAsync(string customerId, int menuItemId)
        {
            logger.LogInformation("Removing favorite for customer {customerId} and menu item {menuItemId}", customerId, menuItemId);
            var favorite = await uow.FavoriteRepo.GetFavoriteByCustomerAndMenuItemAsync(customerId, menuItemId);
            if (favorite == null)
            {
                logger.LogInformation("favourite Not Found");
                throw new NotFoundException("favourite Not Found");
            }
           
                await uow.FavoriteRepo.DeleteAsync(favorite);
                await uow.SaveChangesAsync();
            
        }
    }
}
