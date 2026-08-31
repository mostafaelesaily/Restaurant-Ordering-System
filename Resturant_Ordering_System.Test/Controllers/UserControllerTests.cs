// Test scaffold for UserController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Api_Layer.Controllers;
using Business_Layer.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class UserControllerTests
{
    [Fact]
    public async Task DeleteMyAccount_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<IUserService>();
        var controller = new UserController(service.Object);
        controller.SetUser("user-1");

        var result = await controller.DeleteMyAccount();

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteMyAccount("user-1"), Times.Once);
    }
}
