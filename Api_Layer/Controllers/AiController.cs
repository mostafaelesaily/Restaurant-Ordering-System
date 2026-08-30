using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.DTOs.AiDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        public AiController(IAiService aiService)
        {
            this.aiService = aiService;
        }
        private readonly IAiService aiService;
        [HttpPost("genrate-response")]
        public async Task<IActionResult> GenerateResponse([FromBody] AIRequestDto requestDto)
        {
            var result = await aiService.GenerateResponseAsync(requestDto);
            return Ok(result);
        }

    }
}
