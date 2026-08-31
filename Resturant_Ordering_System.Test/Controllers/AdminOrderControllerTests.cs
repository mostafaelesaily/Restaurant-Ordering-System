// Test scaffold for AdminOrderController.
using Business_Layer.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class AdminOrderControllerTests
{
    [Fact]
    public async Task AssignChef_ReturnsNoContent_AndDelegatesToService()
    {
        var service = new Mock<IAdminOrderService>();
        var controller = new AdminOrderController(service.Object);

        var result = await controller.AssignChef(7, "chef-1");

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.AssignChef(7, "chef-1"), Times.Once);
    }
}
