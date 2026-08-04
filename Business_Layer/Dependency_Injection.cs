using Application.Interfaces.IService;
using Application.Services;
using Business_Layer.Interfaces.IService;
using Business_Layer.Mappings;
using Business_Layer.Services;
using Microsoft.Extensions.DependencyInjection;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Application.Services;
namespace Application
{
    public static class Dependency_Injection
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { },
                  typeof(UserMappingProfile).Assembly
                  );
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IMenuItemService, MenuItemService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<ITableService, TableService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}
