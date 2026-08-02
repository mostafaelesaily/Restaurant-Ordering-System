using Application.DTOs.FavoriteDTOs;
using Business_Layer.DTOs.PaginatedDtos;

namespace Application.Interfaces.IService
{
    public interface IFavoriteService
    {
        Task<PaginatedResultDto<GetFavoriteDto>> GetAllFavoritesAsync(int pageNum, int pageSize);
        Task<GetFavoriteDto?> GetFavoriteByIdAsync(int id);
        Task<GetFavoriteDto> AddFavoriteAsync(string customerId, int menuItemId );
        Task DeleteFavoriteAsync(int id, string userId);
        Task<PaginatedResultDto<GetFavoriteDto>> SearchFavoritesAsync(string searchKey, int pageNum, int pageSize);
        Task<PaginatedResultDto<GetFavoriteDto>> GetFavoritesByCategoryAsync(int categoryId, int pageNum, int pageSize);
        Task<PaginatedResultDto<GetFavoriteDto>> GetFavoritesByMenuItemAsync(int menuItemId, int pageNum, int pageSize);
        Task<PaginatedResultDto<GetFavoriteDto>> GetFavoritesByCustomerIdAsync(string customerId, int pageNum, int pageSize);
        Task<bool> IsFavoriteExistsAsync(string customerId, int menuItemId);
        Task RemoveByMenuItemIdAsync(string customerId, int menuItemId);
    }
}
