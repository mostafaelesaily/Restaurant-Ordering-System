// Test scaffold for NotificationController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class NotificationControllerTests
{
    [Fact]
    public async Task MarkAsRead_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<INotificationService>();
        var controller = new NotificationController(service.Object);
        controller.SetUser("user-1");

        var result = await controller.MarkAsRead(5);

        Assert.IsType<OkObjectResult>(result);
        service.Verify(s => s.MarkAsReadAsync(5, "user-1"), Times.Once);
    }
}
