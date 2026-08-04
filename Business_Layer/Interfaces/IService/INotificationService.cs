using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface INotificationService
    {
        Task <GetNotificationDto> CreateAsync(CreateNotificationDto dto);
        Task<PaginatedResultDto<GetNotificationDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<PaginatedResultDto<GetNotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber, int pageSize);
        Task<GetNotificationDto?> GetByIdAsync(int id);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task DeleteAsync(int notificationId , string userId);
    }
}
