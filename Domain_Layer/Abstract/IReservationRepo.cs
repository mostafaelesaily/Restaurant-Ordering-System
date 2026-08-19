using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Domain.Abstract
{
    public interface IReservationRepo : IGenaricRepo<Reservations , int>
    {
        IQueryable <Reservations> GetUserReservations(string userId);
        IQueryable<Reservations> SearchReservations(string? search);
        IQueryable<Reservations> GetReservationsByTableId(int  tableId);

    }
}
