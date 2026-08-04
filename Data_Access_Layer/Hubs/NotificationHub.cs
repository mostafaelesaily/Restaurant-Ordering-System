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
            if (Context.UserIdentifier is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId
                    , Context.UserIdentifier);
            }
            if (Context.User!.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    "Admin");
            }
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.UserIdentifier is not null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId
                    , Context.UserIdentifier);
            }
            if (Context.User!.IsInRole("Admin"))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    "Admin");
            }
            await base.OnDisconnectedAsync(exception);
    }
    } 
}
