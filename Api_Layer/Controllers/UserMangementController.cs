using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMangementController : ControllerBase
    {
        private readonly IUserManagementService userManagement;

        public UserMangementController(IUserManagementService userManagement)
        {
            this.userManagement = userManagement;
        }
        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager")]
       
        public async Task<IActionResult> GetAllusers(int pageNum , int pageSize)
        {
            var result = await userManagement.GetUsersPaggedAsync(pageNum, pageSize);
            return Ok(result);
        }
        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserInfo(string searchKey)
        {
            var result = await userManagement.GetUserInfo(searchKey);
            return Ok(result);
        }
        [HttpPut("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(UpdateUserDto userDto , string searchKey)
        {
            var result = await userManagement.updateUserAsync(userDto, searchKey);
            return Ok(result);
        }
        [HttpPatch("[action]")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> BanUser(string searchKey) 
        {
            var result = await userManagement.BanUser(searchKey);
            return Ok(result);
        }
        [HttpPatch("[action]")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UnBanUser(string searchKey)
        {
            var result = await userManagement.UnBanUser(searchKey);
            return Ok(result);
        }
        [HttpDelete("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string searchKey) 
        {
            var result =  userManagement.DeleteUser(searchKey);
            return NoContent();
        }
    }

}
