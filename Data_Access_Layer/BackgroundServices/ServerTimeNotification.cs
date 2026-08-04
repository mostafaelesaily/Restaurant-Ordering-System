using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.Interfaces.SignalR;
using Resturant_Ordering_System.Infrastructre.Hubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Infrastructre.BackgroundServices
{
    public class ServerTimeNotification : BackgroundService
    {
        public ServerTimeNotification(ILogger<ServerTimeNotification> logger , IHubContext<NotificationHub,INotificationClient> hubContext)
        {
            this.logger = logger;
            this.hubContext = hubContext;
        }
        private readonly TimeSpan period = TimeSpan.FromSeconds(10);
        private readonly ILogger<ServerTimeNotification> logger;
        private readonly IHubContext<NotificationHub,INotificationClient> hubContext;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))   
            {
                var datatime = DateTime.UtcNow;
                logger.LogInformation("Server time notification: {time}", datatime);
                await hubContext.Clients.All.ReceiveNotification($"Server time: {datatime}");
            }
        }
    }   
}
