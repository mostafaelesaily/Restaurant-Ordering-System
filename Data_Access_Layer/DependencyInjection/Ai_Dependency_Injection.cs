using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Infrastructre.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Infrastructre.DependencyInjection
{
    public static class Ai_Dependency_Injection
    {
        public static IServiceCollection AddAiService
            (this IServiceCollection services
            , IConfiguration configuration)
        {
            services.AddHttpClient<IAiService, AiService>();
            return services;
        }
    }
}
