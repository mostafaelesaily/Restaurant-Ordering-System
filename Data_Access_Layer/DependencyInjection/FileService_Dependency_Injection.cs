using Business_Layer.Interfaces;
using Data_Access_Layer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DependencyInjection
{
    public static class FileService_Dependency_Injection
    {
            public static IServiceCollection AddFileServices(
                this IServiceCollection services)
            {
                services.AddScoped<IFileService, FileService>();

                return services;
            }
        }
    }

