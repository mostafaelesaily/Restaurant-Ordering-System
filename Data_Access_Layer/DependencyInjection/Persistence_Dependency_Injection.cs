using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Data_Access_Layer.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DependencyInjection
{
    public static class Persistence_Dependency_Injection
    {
        public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
        {
             services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(configuration.GetConnectionString("Resturant_Order_System_Db"))
             );
            services.AddScoped(typeof(IGenaricRepo<,>), typeof(MainGenaricRepo<,>));
            services.AddScoped<IUow,Uow>();
            return services;
        }
    }
}
