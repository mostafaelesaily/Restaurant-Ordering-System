using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Data_Access_Layer.Repositories
{
    public class MainGenaricRepo<T, TKey> : IGenaricRepo<T, TKey> where T : class
    {
        private readonly AppDbContext context;
        private readonly DbSet<T> dbSet;

        public MainGenaricRepo(AppDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        {
            var count = await dbSet.CountAsync(filter);
            return count;
        }

        public async Task<T> CreateAsync(T entity)
        {
            var result = await dbSet.AddAsync(entity);
            return result.Entity;
        }
        

        public async Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
        }

        public async Task<T?> FindElementAsync(Expression<Func<T, bool>> filter)
        {
           var item = await dbSet.FirstOrDefaultAsync(filter);
            return item;
        }

       

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<PaginatedResultDto<T>> GetAllPaged(int pageNum, int pageSize, IQueryable<T> query)
        {
            var totalCount = await query.CountAsync();
            var data = await query.Skip((pageNum - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedResultDto<T>
            {
                Data = data,
                PageNumber = pageNum,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }  

        public async Task<T?> GetByIdAsync(TKey id)
        {
            return await dbSet.FindAsync(id);
        }

        public  IQueryable<T> Query()
        {
            return dbSet;
        }

        public async Task<T> UpdateAsync(T entity)
        {
             dbSet.Update(entity); 
             return entity;
        }
    }
}
