using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Data_Access_Layer.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Api_Layer.DependencyInjection
{
    public static class DataAccessDependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer
            (this IServiceCollection services , IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(configuration.GetConnectionString("Resturant_Order_System_Db"))
            );
            services.AddScoped(typeof(IGenaricRepo<,>), typeof(MainGenaricRepo<,>));
            services.AddScoped<IUow, uow>();
            return services;
        }
    }
}
