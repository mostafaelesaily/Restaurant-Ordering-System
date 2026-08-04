using AutoMapper;
using Domain_Layer.Entities;
using Resturant_Ordering_System.Application.DTOs.CartDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Mappings
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {
            CreateMap<AddToCartDto, CartItem>();

            CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.CartItemId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.menuItems.name))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.menuItems.price))
                .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.Quantity * s.menuItems.price));

            CreateMap<Cart, GetCartDto>()
                .ForMember(d => d.CartId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.CartItems, o => o.MapFrom(s => s.Items))
                .ForMember(d => d.TotalPrice,
                    o => o.MapFrom(s => s.Items.Sum(i => i.Quantity * i.menuItems.price)));
        }
    }
}
