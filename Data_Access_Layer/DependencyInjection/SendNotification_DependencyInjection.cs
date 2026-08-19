using Microsoft.Extensions.DependencyInjection;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Infrastructre.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Infrastructre.DependencyInjection
{
    public static class SendNotification_DependencyInjection
    {
      public static IServiceCollection AddSendNotification
      ( this IServiceCollection services)

      {
            services.AddScoped<ISendNotificationService, SendNotificationService>();
            return services;
      }
    }
}
