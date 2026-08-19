using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Domain_Layer.Enums;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.ReservationDtos;
using Resturant_Ordering_System.Application.Interfaces.IService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Resturant_Ordering_System.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly ILogger<ReservationService> logger;
        private readonly INotificationService notificationService;
        private readonly ISendNotificationService sendNotificationService;

        public ReservationService(IUow uow, ICacheService cacheService,
            IMapper mapper, ILogger<ReservationService> logger,
            INotificationService notificationService,
            ISendNotificationService sendNotificationService)
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.logger = logger;
            this.notificationService = notificationService;
            this.sendNotificationService = sendNotificationService;
        }

        public async Task<PaginatedResultDto<ReservationDetailsDto>> GetAllReservation(int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get reservations page {pageNum} size {pageSize}", pageNum, pageSize);
            var cacheKey = $"Get_Reservations_pageNum:{pageNum}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var reservations = await uow.Reservations.GetAllPaged(pageNum, pageSize);
                return new PaginatedResultDto<ReservationDetailsDto>
                {
                    Data = mapper.Map<List<ReservationDetailsDto>>(reservations.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reservations.TotalCount
                };
            });

            return result!;
        }

        public async Task<ReservationDetailsDto> GetReservationById(int reservationId)
        {
            logger.LogInformation("Attempting to get reservation with id {id}", reservationId);
            var reservation = await uow.Reservations.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                logger.LogWarning("Reservation with id {id} not found", reservationId);
                throw new NotFoundException("Reservation not found");
            }
            return mapper.Map<ReservationDetailsDto>(reservation);
        }

        public async Task<PaginatedResultDto<ReservationDetailsDto>> GetUserReservations(string userId, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get reservations for user {userId} page {pageNum} size {pageSize}",
                userId, pageNum, pageSize);
            var cacheKey = $"Reservations_User_{userId}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Reservations.GetUserReservations(userId);
                var reservations = await uow.Reservations.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<ReservationDetailsDto>
                {
                    Data = mapper.Map<List<ReservationDetailsDto>>(reservations.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reservations.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<ReservationDetailsDto>> SearchReservations(string? search, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to search reservations with key {search} page {pageNum} size {pageSize}",
                search, pageNum, pageSize);
            var cacheKey = $"Search_Reservations_{search}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Reservations.SearchReservations(search);
                var reservations = await uow.Reservations.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<ReservationDetailsDto>
                {
                    Data = mapper.Map<List<ReservationDetailsDto>>(reservations.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reservations.TotalCount
                };
            });

            return result!;
        }

        
        public async Task<ReservationDetailsDto> CreateReservation(
        CreateReservationDto reservationCreateDto,
         string customerId)
        {
            logger.LogInformation(
                "Attempting to create reservation for customer {customerId} and table {tableId}",
                customerId,
                reservationCreateDto.TableId);

            var user = await uow.AppUserRepo.GetByIdAsync(customerId);

            if (user == null)
            {
                logger.LogInformation(
                    "Customer with id {customerId} does not exist",
                    customerId);

                throw new NotFoundException("Customer not found");
            }

            var isReserved = await IsReserved(
                reservationCreateDto.TableId,
                reservationCreateDto.ReservationDate,
                reservationCreateDto.Duration);

            if (isReserved)
            {
                logger.LogWarning(
                    "Table with id : {id} Already Reserved For Time {Date} Till {Duration}",
                    reservationCreateDto.TableId,
                    reservationCreateDto.ReservationDate,
                    reservationCreateDto.Duration);

                throw new BadRequestException("Table Is Reserved For This Time");
            }

            var reservation = mapper.Map<Reservations>(reservationCreateDto);

            reservation.custoemerId = customerId;
            reservation.Status = ReservationStatus.Pending;
            reservation.EndTime = reservation.ReservationDate.Add(reservation.Duration);

            await uow.Reservations.CreateAsync(reservation);

            var notification = new CreateNotificationDto
            {
                Title = "Table reservation",
                UserId = customerId,
                Message = $"Your reservation for table {reservationCreateDto.TableId} on " +
                          $"{reservation.ReservationDate} has been created."
            };

            await uow.SaveChangesAsync();

            await notificationService.CreateAsync(notification);

            await sendNotificationService.SendToUserAsync(
                customerId,
                $"Hi {user.UserName} Your reservation has been created successfully.");

            await cacheService.RemoveAsync("Get_Reservations");

            logger.LogInformation(
                "Reservation created successfully with ID {id}",
                reservation.Id);

            return mapper.Map<ReservationDetailsDto>(reservation);
        }

        public async Task<bool> IsReserved(
            int tableId,
            DateTime reservationDate,
            TimeSpan duration)
        {
            var table = await uow.Tables.GetByIdAsync(tableId);

            if (table == null)
            {
                logger.LogWarning(
                    "Table With id : {id} Not Found",
                    tableId);

                throw new NotFoundException("Table Not Found!");
            }

            var reservations = uow.Reservations
                .GetReservationsByTableId(tableId);

            var newStart = reservationDate;
            var newEnd = reservationDate.Add(duration);

            var isReserved = reservations.Any(r =>
                r.ReservationDate < newEnd &&
                r.EndTime > newStart);

            return isReserved;
        }

        public async Task UpdateReservation(int reservationId, UpdateReservationDto reservationUpdateDto, string customerId)
        {
               logger.LogInformation("Attempting to update reservation with id {id}", reservationId);            
                var reservation = await uow.Reservations.GetByIdAsync(reservationId);

            if (reservation == null || reservation.custoemerId != customerId)
            {
                logger.LogWarning("Reservation with id {id} not found for customer {customerId}", reservationId, customerId);
                throw new NotFoundException("Reservation not found");
            }
                mapper.Map(reservationUpdateDto, reservation);
                await uow.Reservations.UpdateAsync(reservation);
                var updatedNotification = new CreateNotificationDto
                {
                    Title = "Update Reservation",
                    UserId = customerId,
                    Message = $"Your reservation for table {reservation.tableId} on" +
                    $" {reservation.ReservationDate} has been updated.",
                };
                await uow.SaveChangesAsync();
                await notificationService.CreateAsync(updatedNotification);
                await sendNotificationService.SendToUserAsync(customerId,
                    $" Your reservation has been updated successfully.");
                await cacheService.RemoveAsync("Get_Reservations");
                logger.LogInformation("Reservation with ID {id} updated successfully", reservationId);
        }

        public async Task DeleteReservation(int reservationId, string customerId)
        {
            logger.LogInformation("Attempting to delete reservation with id {id}", reservationId);
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var reservation = await uow.Reservations.GetByIdAsync(reservationId);
                if (reservation == null || reservation.custoemerId != customerId)
                {
                    logger.LogWarning("Reservation with id {id} not found", reservationId);
                    throw new NotFoundException("Reservation not found");
                }

                await uow.Reservations.DeleteAsync(reservation);
                var deletedNotification = new CreateNotificationDto
                {
                    Title = "Delete Reservation",
                    UserId = customerId,
                    Message = $"Your reservation for table {reservation.tableId} on" +
                    $" {reservation.ReservationDate} has been deleted.",
                };
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await notificationService.CreateAsync(deletedNotification);
                await sendNotificationService.SendToUserAsync(customerId,
                    $" Your reservation has been deleted successfully.");
                await cacheService.RemoveAsync("Get_Reservations");
                logger.LogInformation("Reservation with ID {id} deleted successfully", reservationId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        
    }
}