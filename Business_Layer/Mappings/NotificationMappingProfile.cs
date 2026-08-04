using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Mappings
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notifications, GetNotificationDto>();
            CreateMap<GetNotificationDto, Notifications>();
            CreateMap<Notifications, CreateNotificationDto>();
            CreateMap<CreateNotificationDto, Notifications>();
            
        }
    }
}
