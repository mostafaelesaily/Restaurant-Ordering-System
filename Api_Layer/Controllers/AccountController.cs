using Business_Layer.DTOs.RefreshTokenDtos;
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
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Register(SignUpDto signUpDto)
        {
            var Result = await accountService.Register(signUpDto);
            return Ok(Result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var Result = await accountService.Login(loginDto);
            return Ok(Result);
        }
        [HttpPost("[action]")]

        public async Task<IActionResult> HandleRefreshToken(GenrateRefreshToken refreshToken)
        {
            var Result = await accountService.HandleRefreshTokenAsync(refreshToken);
            return Ok(Result);
        }
        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Result = await accountService.ChangePassword(changePasswordDto, userId);
            return Ok(Result);
        }
        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await accountService.Logout(userId);
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
