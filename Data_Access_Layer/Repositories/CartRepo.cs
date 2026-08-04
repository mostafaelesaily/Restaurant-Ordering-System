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
    public class CartRepo : MainGenaricRepo<Cart,int> ,ICartRepository 
    {
        private readonly DbSet<Cart> dbset;
        public CartRepo(AppDbContext context) : base(context) 
        {
            this.dbset = context.Set<Cart>();
        }
        public IQueryable<Cart> GetAllWithItems()
        {
         return dbset.Include(c => c.Items)
                     .ThenInclude(ci => ci.menuItems);
        }

        public Task<Cart?> GetCartWithItemsAsync(string customerId)
        {
           return dbset.Include(c => c.Items)
                       .ThenInclude(ci => ci.menuItems)
                       .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }
    }
}
