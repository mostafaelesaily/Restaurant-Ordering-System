// Test scaffold for TablesController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class TablesControllerTests
{
    [Fact]
    public async Task DeleteTable_ReturnsNoContent_AndDelegatesToService()
    {
        var service = new Mock<ITableService>();
        var controller = new TablesController(service.Object);

        var result = await controller.DeleteTable(6);

        Assert.IsType<NoContentResult>(result);
        service.Verify(s => s.DeleteTableAsync(6), Times.Once);
    }
}
