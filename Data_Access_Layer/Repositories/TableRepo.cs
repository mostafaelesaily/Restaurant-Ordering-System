using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Infrastructre.Repositories
{
    public class TableRepo : MainGenaricRepo<Tables,int> , ITableRepo
    {
        private readonly DbSet<Tables> _dbSet;
        public TableRepo(AppDbContext context): base(context)
        {
            _dbSet = context.Tables;
        }
        public IQueryable<Tables> Search_Table_With_SearchKey(string searchKey)
        {
            return _dbSet.AsNoTracking().Where(t => t.TableNumber.ToString().Contains(searchKey)
            || t.Capacity.ToString().Contains(searchKey) || (t.QrCode != null) &&
            t.QrCode.Contains(searchKey));    
        }
        public IQueryable<Tables> GetTablesByActiveStatus(bool status)
        {
            return _dbSet.AsNoTracking().Where(t => t.isActive == status);
        }
        public IQueryable<Tables> GetTablesByCapacity(int capacity)
        {
            return _dbSet.AsNoTracking().Where(t => t.Capacity == capacity);
        }
    }
}
