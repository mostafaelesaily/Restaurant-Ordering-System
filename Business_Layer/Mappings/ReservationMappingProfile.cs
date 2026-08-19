using AutoMapper;
using Domain_Layer.Entities;
using Resturant_Ordering_System.Application.DTOs.ReservationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Mappings
{
    public class ReservationMappingProfile : Profile
    {
        public ReservationMappingProfile()
        {
            CreateMap<CreateReservationDto, Reservations>();
            CreateMap<Reservations, CreateReservationDto>();

            CreateMap<Reservations, ReservationDetailsDto>()
                .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.Tables.TableNumber))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<ReservationDetailsDto, Reservations>();

            CreateMap<Reservations, UpdateReservationDto>();
            CreateMap<UpdateReservationDto, Reservations>();
        }
    }
}
