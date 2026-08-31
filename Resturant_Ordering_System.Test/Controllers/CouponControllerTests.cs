// Test scaffold for CouponController.
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api_Layer.Controllers;

namespace Resturant_Ordering_System.Test.Controllers;

public class CouponControllerTests
{
    [Fact]
    public async Task Delete_ReturnsNoContent_AndDelegatesToService()
    {
        var service = new Mock<ICouponService>();
        var controller = new CouponController(service.Object);

        var result = await controller.Delete(4);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteCopoun(4), Times.Once);
    }
}
