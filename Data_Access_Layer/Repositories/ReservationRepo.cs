using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using Resturant_Ordering_System.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Infrastructre.Repositories
{
    public class ReservationRepo : MainGenaricRepo<Reservations, int>, IReservationRepo
    {
        public ReservationRepo(AppDbContext context) : base(context)
        {
            this.dbset = context.Set<Reservations>();
        }        
        private readonly DbSet<Reservations> dbset;
        public IQueryable<Reservations> GetUserReservations(string userId)
        {
        return dbset
       .Include(r => r.User)
       .Include(r => r.Tables)
       .Where(r => r.custoemerId == userId);
        }

        public IQueryable<Reservations> SearchReservations(string? search)
        {
         return dbset
        .Include(r => r.User)
        .Include(r => r.Tables)
        .Where(r =>
         string.IsNullOrEmpty(search) ||
         r.User.UserName.Contains(search) ||
         r.User.Email.Contains(search));
        }

        public IQueryable<Reservations> GetReservationsByTableId(int tableId)
        {
          return dbset.Where(r => r.tableId == tableId);
        }
    }
}
