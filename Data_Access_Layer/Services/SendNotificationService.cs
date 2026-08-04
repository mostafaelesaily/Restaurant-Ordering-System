

using Microsoft.AspNetCore.SignalR;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Application.Interfaces.SignalR;
using Resturant_Ordering_System.Infrastructre.Hubs;
namespace Resturant_Ordering_System.Infrastructre.Services
{
    public class SendNotificationService : ISendNotificationService
    {
        private readonly IHubContext<NotificationHub , INotificationClient> _hubContext;
        public SendNotificationService(IHubContext<NotificationHub , INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendToAllAsync(string message)
        {
            await _hubContext.Clients.All.ReceiveNotification(message);
        }

        public async Task SendToGroupAsync(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).ReceiveNotification(message);
        }

        public async Task SendToUserAsync(string userId, string message)
        {
           await _hubContext.Clients.User(userId).ReceiveNotification(message);
        }
    }
}
