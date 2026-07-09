using Business_Layer.DTOs.PaginatedDtos;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Business_Layer.Interfaces
{
    public interface IGenaricRepo<T , TKey> where T : class
     {
        Task<PaginatedResultDto<T>> GetAllPaged(int pageNum, int pageSize,IQueryable<T> query);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> FindElementAsync(Expression<Func<T, bool>> filter);
        IQueryable<T> Query();
        Task<int> CountAsync(Expression<Func<T, bool>> filter);
        Task<T?> GetByIdAsync(TKey id);
        Task<T>  CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
