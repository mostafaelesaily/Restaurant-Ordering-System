// Test scaffold for FavoriteController.
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api_Layer.Controllers;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Controllers;

public class FavoriteControllerTests
{
    [Fact]
    public async Task RemoveFavoriteByMenuItem_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<IFavoriteService>();
        var controller = new FavoriteController(service.Object);
        controller.SetUser("user-1");

        var result = await controller.RemoveFavoriteByMenuItem(12);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.RemoveByMenuItemIdAsync("user-1", 12), Times.Once);
    }
}
