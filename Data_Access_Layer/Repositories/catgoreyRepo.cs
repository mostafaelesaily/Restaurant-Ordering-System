using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class catgoreyRepo : MainGenaricRepo<Categories,int> , ICatgoreyRepo 
    {
        private readonly DbSet<Categories> dbset;
        public catgoreyRepo(AppDbContext context) : base(context) 
        {
            dbset = context.Set<Categories>();
        }

        public IQueryable<Categories> Search_Catgorey_With_Name_Desc(string searchKey)
        {
            return dbset.Where(c =>
                c.name.Contains(searchKey) ||
                (c.description != null &&
                 c.description.Contains(searchKey)));
        }
    }
}
