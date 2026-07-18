using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.DTOs.UserDTOs;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Business_Layer.Interfaces.IService
{
    public interface IUserManagementService
    {
        Task<PaginatedResultDto<GetUserDto>> GetUsersPaggedAsync
        (  int pageNum , int pageSize );
        Task<GetUserDto> GetUserInfo(string searchKey);
        Task<UpdateUserDto> updateUserAsync(UpdateUserDto updateUserDto,string searchKey);
        Task<bool> BanUser(string searchKey);
        Task<bool> UnBanUser(string searchKey);
        Task DeleteUser (string searchKey);

    }
}

