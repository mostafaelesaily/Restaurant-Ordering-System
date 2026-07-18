using AutoMapper;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces.IService;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper mapper;
        public UserService
        (
            UserManager<AppUser> userManager
            , ILogger<UserService> logger,
            IMapper mapper
            )
        {
            this._userManager = userManager;
            this._logger = logger;
            this.mapper = mapper;
        }
        public async Task DeleteMyAccount(string userId)
        {
            _logger.LogInformation("Attemping to Delete user {userid} profile", userId);
            var user =  await _userManager.FindByIdAsync( userId );
            if (user == null)
            {
                _logger.LogWarning("User With Id {userId} Not Found", userId);
                throw new NotFoundException("User Not Found");
            }
           var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(
           string.Join(", ", result.Errors.Select(e => e.Description))
           );
        }

        public async Task<GetUserDto> GetMyProfile(string userId)
        {
            _logger.LogInformation("Attemping to Get user {userid} profile", userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                _logger.LogWarning("User With Id {userId} Not Found", userId);
                throw new NotFoundException("User Not Found");
            }
            var _user = mapper.Map<GetUserDto>(user);
            return _user;
        }

        public async Task<UpdateUserDto> UpdateMyProfile(UpdateUserDto updateUserDto, string userId)
        {
            _logger.LogInformation("Attemping to Update User {userId} Info",userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User With Id {userId} Not Found", userId);
                throw new NotFoundException("User Not Found");
            }
            mapper.Map(updateUserDto , user);
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("Failed to update user {UserId}", userId);
                throw new Exception("Failed to update user.");
            }

            var result =  mapper.Map<UpdateUserDto>(user);
            return result;
        }
    }
}
