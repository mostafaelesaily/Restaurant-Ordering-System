using Business_Layer.Interfaces;
using Data_Access_Layer.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DependencyInjection
{
    public static class Cache_Dependency_Injection
    {  
            public static IServiceCollection AddCaching(
                this IServiceCollection services,
                IConfiguration configuration)
            {
                services.AddMemoryCache();
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration =
                        configuration.GetConnectionString("Redis");

                    options.InstanceName = "RedisCacheInstance";
                });
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                var connectionString = configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connectionString);
                });
            services.AddScoped<ICacheService, CacheService>();
                return services;
            }
        }
    }

