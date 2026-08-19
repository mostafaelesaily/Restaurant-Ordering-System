using Business_Layer.DTOs.PaginatedDtos;
using Resturant_Ordering_System.Application.DTOs.ReservationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface IReservationService
    {
        Task<PaginatedResultDto<ReservationDetailsDto>> GetAllReservation(int pageNum, int pageSize);
        Task<PaginatedResultDto<ReservationDetailsDto>> GetUserReservations(string userId, int pageNum, int pageSize);
        Task<ReservationDetailsDto> GetReservationById(int reservationId);
        Task<PaginatedResultDto<ReservationDetailsDto>> SearchReservations(string? search, int pageNum, int pageSize);
        Task<ReservationDetailsDto> CreateReservation(CreateReservationDto reservationCreateDto , string customerId);
        Task<bool> IsReserved(int tableId, DateTime ReservationDate , TimeSpan Duration );
        Task UpdateReservation(int reservationId, UpdateReservationDto reservationUpdateDto , string customerId);
        Task DeleteReservation(int reservationId, string customerId);
    }
}
