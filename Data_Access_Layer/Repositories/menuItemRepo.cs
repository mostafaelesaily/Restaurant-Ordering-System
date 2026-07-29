using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Repositories
{
    public class menuItemRepo : MainGenaricRepo<MenuItems,int> , IMenuItemRepo
    {
        private readonly DbSet<MenuItems> dbset;
        public menuItemRepo(AppDbContext context) : base(context)
        {
            dbset = context.Set<MenuItems>();
        }

        public IQueryable<MenuItems> Search_MenuItem_With_Name_Desc(string searchKey)
        {
            return dbset.Where(m =>
                m.name.Contains(searchKey) ||
                (m.description != null &&
                 m.description.Contains(searchKey)));
        }
        public IQueryable<MenuItems> GetCategoreyMenuItems(int categoreyId)
        {
            return dbset.Where(ci => ci.categoryId == categoreyId);
        }
    }
}
