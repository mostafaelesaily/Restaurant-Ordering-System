using Application.DTOs.FavoriteDTOs;
using AutoMapper;
using Domain_Layer.Entities;

namespace Application.Mappings
{
    public class FavoriteMappingProfile : Profile
    {
        public FavoriteMappingProfile()
        {
            CreateMap<Favorite, CreateFavoriteDto>();
            CreateMap<CreateFavoriteDto, Favorite>();
            CreateMap<Favorite, GetFavoriteDto>()
                .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItems.name))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.MenuItems.categoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.MenuItems.categories.name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.MenuItems.price));
            CreateMap<GetFavoriteDto, Favorite>();
            CreateMap<Favorite, UpdateFavoriteDto>();
            CreateMap<UpdateFavoriteDto, Favorite>();
        }
    }
}
