// Test scaffold for AccountController.
using Api_Layer.Controllers;
using Business_Layer.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Controllers;

public class AccountControllerTests
{
    [Fact]
    public async Task Logout_ReturnsOk_AndPassesAuthenticatedUserId()
    {
        var service = new Mock<IAccountService>();
        var controller = new AccountController(service.Object);
        controller.SetUser("user-1");

        var result = await controller.Logout();

        Assert.IsType<OkObjectResult>(result);
        service.Verify(s => s.Logout("user-1"), Times.Once);
    }
}
