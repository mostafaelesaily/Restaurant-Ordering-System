using Business_Layer.Interfaces;
using Domain_Layer.Entities;

namespace Domain_Layer.Abstract
{
    public interface IFavoriteRepo : IGenaricRepo<Favorite, int>
    {
        IQueryable<Favorite> SearchFavoritesAsync(string searchKey);
        IQueryable<Favorite> GetFavoritesByCategoryAsync(int categoryId);
        IQueryable<Favorite> GetFavoritesByMenuItemsAsync(int menuItemId);
        IQueryable<Favorite> GetFavoritesByCustomerIdAsync(string customerId);
        Task<Favorite?> GetFavoriteByCustomerAndMenuItemAsync(string customerId, int menuItemId);
        Task<Favorite?> GetFavoriteByIdAndCustomerIdAsync(int favoriteId, string customerId);
    }
}
