using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface ISendNotificationService
    {
        Task SendToUserAsync(string userId, string message);

        Task SendToGroupAsync(string groupName, string message);

        Task SendToAllAsync(string message);
    }
}
