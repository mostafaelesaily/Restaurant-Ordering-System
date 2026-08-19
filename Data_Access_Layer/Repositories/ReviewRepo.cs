using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using Resturant_Ordering_System.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Infrastructre.Repositories
{
    public class ReviewRepo : MainGenaricRepo<Reviews, int>, IReviewRepo
    {
        private readonly DbSet<Reviews> dbset;
        public ReviewRepo(AppDbContext context) : base(context)
        {
            this.dbset = context.Set<Reviews>();
        }

        public IQueryable<Reviews> GetMenuItemReviews(int menuItemId)
        {
            return dbset.AsNoTracking()
                .Include(u => u.User)
                .Include(m => m.MenuItems)
                .Where(r => r.MenuItemId == menuItemId);
                
        }

        public IQueryable<Reviews> GetUserReviews(string userId)
        {
            return dbset.AsNoTracking()
                .Include(u => u.User)
                .Include(mi => mi.MenuItems)
                .Where(r => r.CustomerId == userId);    
        }

        public IQueryable<Reviews> SearchReviews(string? search)
        {
            return dbset.AsNoTracking()
                .Include(r => r.MenuItems)
                .Include(r => r.User)
                .Where(r =>
                    string.IsNullOrWhiteSpace(search) ||
                    r.Comment.Contains(search) ||
                    r.Rating.ToString().Contains(search) ||
                    r.User.UserName.Contains(search) ||
                    r.User.Email.Contains(search));
        }
        public IQueryable<Reviews> GetReviewsWithDetails()
        {
            return dbset
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.MenuItems);
        }
        public async Task<Reviews?> GetReviewWithDetails(int reviewId)
{
         return await dbset
        .AsNoTracking()
        .Include(r => r.User)
        .Include(r => r.MenuItems)
        .FirstOrDefaultAsync(r => r.Id == reviewId);
}
    }
}
