namespace Resturant_Ordering_System.Application.Interfaces.SignalR
{
    public interface INotificationClient
    {
        Task ReceiveNotification(string message);
    }

}

