// Test scaffold for ReviewController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class ReviewControllerTests
{
    [Fact]
    public async Task DeleteReview_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<IReviewService>();
        var controller = new ReviewController(service.Object);
        controller.SetUser("customer-1");

        var result = await controller.DeleteReview(9);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteReview(9, "customer-1"), Times.Once);
    }
}
