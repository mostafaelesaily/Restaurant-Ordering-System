using Application.DTOs.MenuItemDTOs;
using AutoMapper;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Mappings
{
    public class MenuItemMappingProfile : Profile
    {
        public MenuItemMappingProfile()
        {
            CreateMap<MenuItems, CreateMenuItemDto>();
            CreateMap<CreateMenuItemDto, MenuItems>();
            CreateMap<MenuItems, GetMenuItemDto>();
            CreateMap<GetMenuItemDto, MenuItems>();
            CreateMap<MenuItems, UpdateMenuItemDto>();
            CreateMap<UpdateMenuItemDto, MenuItems>();
        }
    }
}
