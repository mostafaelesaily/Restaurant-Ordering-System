using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.Interfaces.IService;


namespace Resturant_Ordering_System.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly ILogger<NotificationService> logger;

        public NotificationService(
            IUow uow,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<GetNotificationDto> CreateAsync(CreateNotificationDto dto)
        {
            logger.LogInformation(
                "Attempting to create notification for user {userId}",
                dto.UserId);
            var user = await uow.AppUserRepo.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                logger.LogWarning("User with id {userId} not found", dto.UserId);
                throw new NotFoundException("User not found");
            }

            var notification = mapper.Map<Notifications>(dto);

            await uow.Notifications.CreateAsync(notification);
            await uow.SaveChangesAsync();

            await cacheService.RemoveAsync("Get_Notifications");
            logger.LogInformation(
                "Notification created successfully for user {userId}",
                dto.UserId);
            return mapper.Map<GetNotificationDto>(notification);
        }
        public async Task<PaginatedResultDto<GetNotificationDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            logger.LogInformation(
                "Attempting to get notifications page {pageNumber} size {pageSize}",
                pageNumber,
                pageSize);

            var cacheKey = $"Get_Notifications_pageNum:{pageNumber}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var notifications = await uow.Notifications.GetAllPaged(pageNumber, pageSize);

                return new PaginatedResultDto<GetNotificationDto>
                {
                    Data = mapper.Map<List<GetNotificationDto>>(notifications.Data),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = notifications.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<GetNotificationDto>> GetUserNotificationsAsync(
            string userId,
            int pageNumber,
            int pageSize)
        {
            logger.LogInformation(
                "Attempting to get notifications for user {userId} page {pageNumber} size {pageSize}",
                userId,
                pageNumber,
                pageSize);

            var cacheKey = $"Get_User_Notifications_{userId}_pageNum:{pageNumber}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Notifications.GetUserNotifications(userId);
                var notifications = await uow.Notifications.GetAllPaged(pageNumber, pageSize, query);

                return new PaginatedResultDto<GetNotificationDto>
                {
                    Data = mapper.Map<List<GetNotificationDto>>(notifications.Data),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = notifications.TotalCount
                };
            });

            return result!;
        }

        public async Task<GetNotificationDto?> GetByIdAsync(int id)
        {
            logger.LogInformation("Attempting to get notification with id {id}", id);

            var notification = await uow.Notifications.GetByIdAsync(id);

            if (notification == null)
            {
                logger.LogWarning("Notification with id {id} not found", id);
                throw new NotFoundException("Notification not found");
            }

            return mapper.Map<GetNotificationDto>(notification);
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            logger.LogInformation("Attempting to mark notification {notificationId} as read", notificationId);
            var user = await uow.AppUserRepo.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogInformation($"Unable to mark user {userId}");
                throw new NotFoundException("User not found");
            }
            var notification = await uow.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
            {
                logger.LogWarning("Notification with id {notificationId} not found", notificationId);
                throw new NotFoundException("Notification not found");
            }

            notification.IsRead = true;
            await uow.Notifications.UpdateAsync(notification);
            await uow.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_Notifications");
            logger.LogInformation("Notification {notificationId} marked as read successfully", notificationId);
        }

        public async Task DeleteAsync(int notificationId, string userId)
        {
            logger.LogInformation("Attempting to delete notification with id {notificationId}", notificationId);
            var user = await uow.AppUserRepo.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogInformation($"Unable to delete user {userId}");
                throw new NotFoundException("User not found");
            }
            var notification = await uow.Notifications.GetByIdAsync(notificationId);

            if (notification == null)
            {
                logger.LogWarning("Notification with id {notificationId} not found", notificationId);
                throw new NotFoundException("Notification not found");
            }
            await uow.Notifications.DeleteAsync(notification);
            await uow.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_Notifications");
            logger.LogInformation("Notification with id {notificationId} deleted successfully", notificationId);


        }
    }
}