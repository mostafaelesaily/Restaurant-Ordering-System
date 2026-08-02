using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FavoriteRepo : MainGenaricRepo<Favorite, int>, IFavoriteRepo
    {
        private readonly DbSet<Favorite> dbset;

        public FavoriteRepo(AppDbContext context) : base(context)
        {
            dbset = context.Set<Favorite>();
        }

        public IQueryable<Favorite> SearchFavoritesAsync(string searchKey)
        {
            return dbset.AsNoTracking()
                .Include(f => f.MenuItems)
                .ThenInclude(m => m.categories)
                .Where(x => x.MenuItems.name.ToLower().Contains(searchKey.ToLower()) ||
                 (x.MenuItems.description != null) &&
                 x.MenuItems.description.ToLower().Contains(searchKey.ToLower()));
        }

        public IQueryable<Favorite> GetFavoritesByCategoryAsync(int categoryId)
        {
            return dbset.AsNoTracking()
                .Include(f => f.MenuItems)
                .ThenInclude(m => m.categories)
                .Where(x => x.MenuItems.categoryId == categoryId);
        }

        public IQueryable<Favorite> GetFavoritesByMenuItemsAsync(int menuItemId)
        {
            return dbset.AsNoTracking()
                .Include(f => f.MenuItems)
                .ThenInclude(m => m.categories)
                .Where(x => x.MenuItemId == menuItemId);
        }
        public async Task<Favorite?> GetFavoriteByCustomerAndMenuItemAsync(string customerId, int menuItemId)
        {
            return await dbset
                .FirstOrDefaultAsync(x => x.CustomerId == customerId &&
                                          x.MenuItemId == menuItemId);
        }
        public async Task<Favorite?> GetFavoriteByIdAndCustomerIdAsync(int favoriteId, string customerId)
        {
            return await dbset
                .Include(f => f.MenuItems)
                .ThenInclude(m => m.categories)
                .FirstOrDefaultAsync(x => x.Id == favoriteId && x.CustomerId == customerId);
        }
        public IQueryable<Favorite> GetFavoritesByCustomerIdAsync(string customerId)
        {
            return dbset.AsNoTracking()
                .Include(f => f.MenuItems)
                .ThenInclude(m => m.categories)
                .Where(x => x.CustomerId == customerId);
        }
    }
}