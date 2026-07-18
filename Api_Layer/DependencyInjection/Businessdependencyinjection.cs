using Business_Layer.Interfaces;
using Business_Layer.Interfaces.IService;
using Business_Layer.Services;
using Data_Access_Layer.Cache;

namespace Api_Layer.DependencyInjection
{
    public static class Businessdependencyinjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services) 
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService,UserService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}
