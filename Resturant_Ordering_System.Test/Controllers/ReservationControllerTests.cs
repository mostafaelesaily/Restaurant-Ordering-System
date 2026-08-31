// Test scaffold for ReservationController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class ReservationControllerTests
{
    [Fact]
    public async Task DeleteReservation_ReturnsNoContent_AndUsesCurrentUser()
    {
        var service = new Mock<IReservationService>();
        var controller = new ReservationController(service.Object);
        controller.SetUser("customer-1");

        var result = await controller.DeleteReservation(11);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteReservation(11, "customer-1"), Times.Once);
    }
}
