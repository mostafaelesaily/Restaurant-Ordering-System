using Microsoft.Extensions.DependencyInjection;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Infrastructre.Services;

namespace Resturant_Ordering_System.Infrastructre.DependencyInjection
{
    public static class Gmail_DependencyInjection
    {
        public static IServiceCollection AddGmailService(
            this IServiceCollection services)
        {
            services.AddScoped<IGmailService, GmailService>();
            return services;
        }
    }
}
