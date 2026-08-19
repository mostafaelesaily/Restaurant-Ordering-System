using AutoMapper;
using Domain_Layer.Entities;
using Resturant_Ordering_System.Application.DTOs.ReviewDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Mappings
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            CreateMap<CreateReviewDto, Reviews>();
            CreateMap<Reviews, CreateReviewDto>();

            CreateMap<Reviews, ReviewDetailsDto>()
                .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItems.name))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<ReviewDetailsDto, Reviews>();

            CreateMap<Reviews, UpdateReviewDto>();
            CreateMap<UpdateReviewDto, Reviews>();
        }
    }
}
