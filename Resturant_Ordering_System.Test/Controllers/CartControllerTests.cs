// Test scaffold for CartController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class CartControllerTests
{
    [Fact]
    public async Task ClearCart_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<ICartService>();
        var controller = new CartController(service.Object);
        controller.SetUser("customer-1");

        var result = await controller.ClearCart();

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.ClearCartAsync("customer-1"), Times.Once);
    }
}
