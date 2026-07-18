using Business_Layer.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Interfaces.IService
{
    public interface IUserService
    {
        Task<GetUserDto> GetMyProfile(string userId);
        Task<UpdateUserDto> UpdateMyProfile(UpdateUserDto updateUserDto , string userId);
        Task DeleteMyAccount (string userId);
    }
}
