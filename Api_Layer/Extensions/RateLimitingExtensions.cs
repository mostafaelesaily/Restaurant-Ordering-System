using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Resturant_Ordering_System.Api_Layer.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddRateLimiting(
            this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // 1. Fixed Window
                options.AddFixedWindowLimiter("Fixed", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });

                // 2. Sliding Window
                options.AddSlidingWindowLimiter("Sliding", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.SegmentsPerWindow = 6;
                    limiterOptions.QueueLimit = 0;
                });

                // 3. Token Bucket
                options.AddTokenBucketLimiter("TokenBucket", limiterOptions =>
                {
                    limiterOptions.TokenLimit = 10;
                    limiterOptions.TokensPerPeriod = 5;
                    limiterOptions.ReplenishmentPeriod =
                        TimeSpan.FromSeconds(30);
                    limiterOptions.QueueLimit = 0;
                    limiterOptions.AutoReplenishment = true;
                });

                // 4. Concurrency
                options.AddConcurrencyLimiter("Concurrency", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.QueueLimit = 0;
                });

                // Rate Limiting With IP

                // 5. Fixed Window - Per IP
                options.AddPolicy("FixedByIp", httpContext =>
                {
                    var ip =
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                });

                // 6. Sliding Window - Per IP
                options.AddPolicy("SlidingByIp", httpContext =>
                {
                    var ip =
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueLimit = 0
                        });
                });

                // 7. Token Bucket - Per IP
                options.AddPolicy("TokenBucketByIp", httpContext =>
                {
                    var ip =
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: ip,
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            TokensPerPeriod = 5,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(30),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });

                // 8. Concurrency - Per IP
                options.AddPolicy("ConcurrencyByIp", httpContext =>
                {
                    var ip =
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetConcurrencyLimiter(
                        partitionKey: ip,
                        factory: _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = 5,
                            QueueLimit = 0
                        });
                });
            });

            return services;
        }
    }
}