using Application.DTOs.CouponDTOs;
using AutoMapper;
using Domain_Layer.Entities;

namespace Application.Mappings
{
    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {
            CreateMap<Coupon, CreateCouponDto>();
            CreateMap<CreateCouponDto, Coupon>();
            CreateMap<Coupon, GetCouponDto>();
            CreateMap<GetCouponDto, Coupon>();
            CreateMap<Coupon, UpdateCouponDto>();
            CreateMap<UpdateCouponDto, Coupon>();
        }
    }
}
