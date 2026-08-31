// Test scaffold for MenuItemController.
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api_Layer.Controllers;

namespace Resturant_Ordering_System.Test.Controllers;

public class MenuItemControllerTests
{
    [Fact]
    public async Task DeleteMenuItem_ReturnsNoContent_AndDelegatesToService()
    {
        var service = new Mock<IMenuItemService>();
        var controller = new MenuItemController(service.Object);

        var result = await controller.DeleteMenuItem(8);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteAsync(8), Times.Once);
    }
}
