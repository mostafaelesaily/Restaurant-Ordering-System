using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Application.Interfaces.SignalR;
namespace Resturant_Ordering_System.Infrastructre.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId)
           .ReceiveNotification($"Connection Work Fine For : " +
           $"{
           Context.User?.Identity?.Name    
           }");
           await base.OnConnectedAsync();
        }
    }
}
