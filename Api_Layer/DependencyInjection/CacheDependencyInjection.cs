using Business_Layer.Interfaces;
using Data_Access_Layer.Cache;

namespace Api_Layer.DependencyInjection
{
    public static class CacheDependencyInjection
    {
        public static IServiceCollection AddCacheService(this IServiceCollection services 
            , IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "RedisCacheInstance";
            });
            return services;
        }
    }
}
