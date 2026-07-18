using AutoMapper;
using Business_Layer.DTOs.UserDTOs;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<AppUser, SignUpDto>();
            CreateMap<SignUpDto, AppUser>();
            CreateMap<AppUser, LoginDto>();
            CreateMap<LoginDto, AppUser>();
            CreateMap<AppUser,GetUserDto>();
            CreateMap<GetUserDto, AppUser>();
            CreateMap<AppUser, ChangePasswordDto>();
            CreateMap<ChangePasswordDto, AppUser>();
            CreateMap<AppUser, UpdateUserDto>();
            CreateMap<UpdateUserDto, AppUser>();
            
        }
    }
}
