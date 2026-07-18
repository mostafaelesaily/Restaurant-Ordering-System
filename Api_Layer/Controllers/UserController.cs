using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }
        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await userService.GetMyProfile(userId);
            return Ok(result);
        }
        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile(UpdateUserDto updateUserDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await userService.UpdateMyProfile(updateUserDto, userId);
            return Ok(result);
        }
        [HttpDelete("[action]")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await userService.DeleteMyAccount(userId);
            return NoContent();
        }
    }
}
