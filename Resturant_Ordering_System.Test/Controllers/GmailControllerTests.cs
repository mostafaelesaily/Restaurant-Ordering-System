// Test scaffold for GmailController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class GmailControllerTests
{
    [Fact]
    public async Task OAuthCallback_ReturnsOk_AndPassesCode()
    {
        var service = new Mock<IGmailService>();
        var controller = new GmailController(service.Object);

        var result = await controller.OAuthCallback("auth-code");

        Assert.IsType<OkObjectResult>(result);
        service.Verify(s => s.HandleOAuthCallbackAsync("auth-code"), Times.Once);
    }
}
