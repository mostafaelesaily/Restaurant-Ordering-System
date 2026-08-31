// Test scaffold for UserMangementController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Api_Layer.Controllers;
using Business_Layer.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class UserMangementControllerTests
{
    [Fact]
    public async Task DeleteUser_ReturnsNoContent_AndDelegatesToService()
    {
        var service = new Mock<IUserManagementService>();
        var controller = new UserMangementController(service.Object);

        var result = await controller.DeleteUser("user-1");

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteUser("user-1"), Times.Once);
    }
}
