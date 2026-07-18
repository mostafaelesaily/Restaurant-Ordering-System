using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null);

        Task RemoveAsync(string key);

        Task<bool> ExistsAsync(string key);

        Task<T?> GetOrSetAsync<T>(
            string key,
            Func<Task<T?>> factory,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null);

    }
}
