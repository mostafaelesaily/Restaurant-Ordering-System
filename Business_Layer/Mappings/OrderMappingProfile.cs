using AutoMapper;
using Domain_Layer.Entities;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;

namespace Resturant_Ordering_System.Application.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<CreateOrderDto, Orders>();

            CreateMap<CreateOrderByAdminDto, Orders>()
                .ForMember(d => d.CheifId, o => o.MapFrom(s => s.CheifId));

            CreateMap<CreateOrderItemDto, OrderItems>();

            CreateMap<Orders, OrderDetailsDto>()
                .ForMember(d => d.OrderId, o => o.MapFrom(s => s.id))
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.AppUser.UserName))
                .ForMember(d => d.ChefName, o => o.MapFrom(s => s.Cheif != null ? s.Cheif.UserName : null))
                .ForMember(d => d.DeliveryName, o => o.MapFrom(s => s.DeliveryUser != null ? s.DeliveryUser.UserName : null))
                .ForMember(d => d.TableNumber, o => o.MapFrom(s => s.Tables != null ? s.Tables.TableNumber : (int?)null))
                .ForMember(d => d.CouponCode, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Code : null))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.orderItems));

            CreateMap<OrderItems, OrderItemDto>()
                .ForMember(d => d.MenuItemName, o => o.MapFrom(s => s.menuItems.name))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.unitPrice));

            CreateMap<Orders, OrderSummaryDto>()
                .ForMember(d => d.orderId, o => o.MapFrom(s => s.id))
                .ForMember(d => d.customerName, o => o.MapFrom(s => s.AppUser.UserName));
        }
    }
}