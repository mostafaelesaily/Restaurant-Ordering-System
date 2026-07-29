using Application.DTOs.CatgoreyDTOs;
using AutoMapper;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Mappings
{
    public class CatgoreyMappingProfile : Profile
    {
        public CatgoreyMappingProfile()
        {
            CreateMap<Categories, CreateCatgoreyDto>();
            CreateMap<CreateCatgoreyDto, Categories>();
            CreateMap<Categories, GetCatgoreyDto>();
            CreateMap<GetCatgoreyDto, Categories>();
            CreateMap<Categories, UpdateCategoryDto>();
            CreateMap<UpdateCategoryDto, Categories>();
        }
    }
}
