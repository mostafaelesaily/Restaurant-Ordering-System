using Business_Layer.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Data_Access_Layer.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache cache;
        private readonly IConnectionMultiplexer redis;
        public CacheService(IDistributedCache cache , IConnectionMultiplexer redis)
        {
            this.cache = cache;
            this.redis = redis;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await cache.GetStringAsync(key) != null;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await cache.GetStringAsync(key);

            return value != null
                ? JsonConvert.DeserializeObject<T>(value)
                : default;
        }

        public async Task<T?> GetOrSetAsync<T>(
            string key,
            Func<Task<T?>> factory,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null)
        {
            var value = await GetAsync<T>(key);

            if (value != null)
                return value;

            value = await factory();

            if (value != null)
                await SetAsync(key, value, slidingExpiration, absoluteExpiration);

            return value;
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(5),
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromHours(1)
            };

            var json = JsonConvert.SerializeObject(value);

            await cache.SetStringAsync(key, json, options);
        }

        public async Task RemoveAsync(string Pattern)
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            var database = redis.GetDatabase();
            foreach (var key in server.Keys(pattern : $"{Pattern}*"))
            {
                await database.KeyDeleteAsync(key);
            }

        }
    }
}