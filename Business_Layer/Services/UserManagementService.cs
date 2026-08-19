using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Business_Layer.Interfaces.IService;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.UserDTOs;
using Resturant_Ordering_System.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly ILogger<UserManagementService> logger;
        private readonly UserManager<AppUser> userManager;

        public UserManagementService
        (IUow uow , ICacheService cacheService,
        IMapper mapper,ILogger<UserManagementService> logger,
        UserManager<AppUser> userManager
            
            )
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
        }

        public async Task<PaginatedResultDto<GetUserDto>> GetUsersPaggedAsync
        ( int pageNum, int pageSize)
        {
            logger.LogInformation("Attemping To Get Users With Paggenation");
            var cacheKey = $"Get_Users_pageNum:" +
                  $"{pageNum}_pageSize:{pageSize}";
           
            var result = await cacheService.GetOrSetAsync
                (
                cacheKey,
                async() => {
                   
                    var users = await uow.AppUserRepo.GetAllPaged
                        (pageNum, pageSize);
                    return new PaginatedResultDto<GetUserDto>
                    {
                        Data = mapper.Map<List<GetUserDto>>(users.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = users.TotalCount
                    };
                }
                );

            return result!;
        }

        public async Task<GetUserDto> GetUserInfo(string searchKey)
        {
            logger.LogInformation("Attemping To Get User {searchKey}", searchKey);
            var user = await uow.AppUserRepo.FindElementAsync(s => s.UserName
            == searchKey || 
            s.Email == searchKey || 
            s.PhoneNumber == searchKey ||
            s.Id == searchKey
            );
            if (user == null) 
            {
                logger.LogWarning("user {searchKey} Not Found", searchKey);
                throw new NotFoundException("user Not Found");
            }
            return mapper.Map<GetUserDto>(user);
        }

        public async Task<UpdateUserDto> updateUserAsync(UpdateUserDto updateUserDto, string searchKey)
        {
            logger.LogInformation("Attemping To Get User {searchKey}", searchKey);
            var user = await uow.AppUserRepo.FindElementAsync(s => s.UserName
            == searchKey ||
            s.Email == searchKey ||
            s.PhoneNumber == searchKey ||
            s.Id == searchKey
            );
            if (user == null)
            {
                logger.LogWarning("user {searchKey} Not Found", searchKey);
                throw new NotFoundException("user Not Found");
            }
           var updatedUser =  mapper.Map(updateUserDto, user);
           await uow.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_Users");
            await cacheService.RemoveAsync("Get_User");
            return mapper.Map<UpdateUserDto>(updatedUser); 
        }

        public async Task<bool> BanUser(string searchKey)
        {
            var user = await uow.AppUserRepo.FindElementAsync(x =>
                x.Id == searchKey ||
                x.UserName == searchKey ||
                x.Email == searchKey ||
                x.PhoneNumber == searchKey);

            if (user == null)
                throw new NotFoundException("User Not Found");

            var result = await userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.MaxValue);
            if (!result.Succeeded)
            {
                throw new BadRequestException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await cacheService.RemoveAsync("Get_Users");
            await cacheService.RemoveAsync("Get_User");
            return result.Succeeded;
        }
        public async Task<bool> UnBanUser(string searchKey)
        {
            var user = await uow.AppUserRepo.FindElementAsync(x =>
              x.Id == searchKey ||
              x.UserName == searchKey ||
              x.Email == searchKey ||
              x.PhoneNumber == searchKey);

            if (user == null)
                throw new NotFoundException("User Not Found");
            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            if (!result.Succeeded)
            {
                throw new BadRequestException(string.Join(", ",result.Errors.Select(e => e.Description)));
            }
            await cacheService.RemoveAsync("Get_Users");
            await cacheService.RemoveAsync("Get_User");
            return result.Succeeded;

        }
        public async Task DeleteUser(string searchKey)
        {
            var user = await uow.AppUserRepo.FindElementAsync(x =>
             x.Id == searchKey ||
             x.UserName == searchKey ||
             x.Email == searchKey ||
             x.PhoneNumber == searchKey);

            if (user == null)
                throw new NotFoundException("User Not Found");
            await userManager.DeleteAsync(user);
            await uow.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_Users");
            await cacheService.RemoveAsync("Get_User");
        }

        public async Task<List<GetUserDto>> GetUsersByRoleAsync(string role)
        {
            logger.LogInformation("Attempting to get users with role {Role}", role);

            var users = await userManager.GetUsersInRoleAsync(role);

            return mapper.Map<List<GetUserDto>>(users);
        }

        public async Task<AddEmployeeResponseDto> AddEmployee(EmployeeDto employeeDto)
        {
            var temporaryPassword = PasswordGenerator.
                Genrate_Temporary_Password();
            var user =  mapper.Map<AppUser>(employeeDto);
            user.MustChangePassword = true;
            var result  = await userManager.CreateAsync(user,temporaryPassword);
            if (!result.Succeeded)
            {  
                throw new Exception
                    (
                     string.Join(" ,", result.Errors.Select(e => e.Description))
                    );
            }
            var roleName = employeeDto.Role.ToString();
            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);

                throw new Exception(
                    string.Join(", ", roleResult.Errors.Select(e => e.Description))
                );
            }
            var userDto = mapper.Map<GetUserDto>(user);
            return new AddEmployeeResponseDto
            {
                getUserDto = userDto,
                TemporaryPassword = temporaryPassword,
            };
        }
    }
}
