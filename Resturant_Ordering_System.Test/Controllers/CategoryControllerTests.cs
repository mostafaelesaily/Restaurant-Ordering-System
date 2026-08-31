// Test scaffold for CategoryController.
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api_Layer.Controllers;

namespace Resturant_Ordering_System.Test.Controllers;

public class CategoryControllerTests
{
    [Fact]
    public async Task Delete_ReturnsNoContent_AndDelegatesToService()
    {
        var service = new Mock<ICategoryService>();
        var menuItems = new Mock<IMenuItemService>();
        var controller = new CategoryController(service.Object, menuItems.Object);

        var result = await controller.Delete(3);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteAsync(3), Times.Once);
    }
}
