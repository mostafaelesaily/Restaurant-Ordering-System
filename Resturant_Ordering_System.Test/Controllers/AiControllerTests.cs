// Test scaffold for AiController.
using Microsoft.AspNetCore.Mvc;
using Moq;
using Resturant_Ordering_System.Api_Layer.Controllers;
using Resturant_Ordering_System.Application.DTOs.AiDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Test.Controllers;

public class AiControllerTests
{
    [Fact]
    public async Task GenerateResponse_ReturnsServiceResponse()
    {
        var expected = new AIResponseDto { Content = "response" };
        var service = new Mock<IAiService>();
        service.Setup(s => s.GenerateResponseAsync(It.IsAny<AIRequestDto>()))
            .ReturnsAsync(expected);
        var controller = new AiController(service.Object);
        var request = new AIRequestDto { Request = "hello" };

        var result = await controller.GenerateResponse(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        service.Verify(s => s.GenerateResponseAsync(request), Times.Once);
    }
}
