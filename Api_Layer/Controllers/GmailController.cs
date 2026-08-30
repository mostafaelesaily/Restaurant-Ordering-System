using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.DTOs.EmailDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GmailController : ControllerBase
    {
        public GmailController(IGmailService gmailService)
        {
            this.gmailService = gmailService;
        }

        private readonly IGmailService gmailService;

        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            var authorizationUrl = await gmailService.GetAuthorizationUrlAsync();
            return Ok(authorizationUrl);
        }

        [HttpGet("oauth2/callback")]
        public async Task<IActionResult> OAuthCallback([FromQuery] string code)
        {
            await gmailService.HandleOAuthCallbackAsync(code);
            return Ok("Gmail authorization completed successfully.");
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendEmailRequestDto request)
        {
            var result = await gmailService.SendEmailAsync(request);
            return Ok(result);
        }
    }
}
