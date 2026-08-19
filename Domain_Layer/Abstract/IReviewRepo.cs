using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Domain.Abstract
{
    public interface IReviewRepo : IGenaricRepo<Reviews, int>
    {
        public IQueryable<Reviews> GetMenuItemReviews(int menuItemId);
        public IQueryable<Reviews> GetUserReviews(string userId);
        public IQueryable<Reviews> SearchReviews(string ? search);
        public IQueryable<Reviews> GetReviewsWithDetails();
        public  Task<Reviews?> GetReviewWithDetails(int reviewId);
        
    }
}
