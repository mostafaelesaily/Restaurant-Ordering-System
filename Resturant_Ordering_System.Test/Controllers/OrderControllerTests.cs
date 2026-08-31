// Test scaffold for OrderController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class OrderControllerTests
{
    [Fact]
    public async Task CancelOrder_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<IOrderService>();
        var controller = new OrderController(service.Object);
        controller.SetUser("customer-1");

        var result = await controller.CancelOrder(10);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.CancelOrder(10, "customer-1"), Times.Once);
    }
}
