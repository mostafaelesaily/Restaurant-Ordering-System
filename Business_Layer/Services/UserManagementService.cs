using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Business_Layer.Interfaces.IService;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
            var _query = uow.AppUserRepo.Query();
            var users = await uow.AppUserRepo.GetAllPaged
                (pageNum, pageSize,_query);
            var usersDto = await cacheService.GetOrSetAsync
                (
                cacheKey,
                async() => { return mapper.Map <List<GetUserDto>>(users.Data); }
                );
             return new PaginatedResultDto<GetUserDto>
            {
                Data = usersDto,
                PageNumber = users.PageNumber,
                PageSize = users.PageSize,
                TotalCount = users.TotalCount
            };

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
           await uow.SaveChanges();
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
            await uow.SaveChanges();
        }

    }
}
