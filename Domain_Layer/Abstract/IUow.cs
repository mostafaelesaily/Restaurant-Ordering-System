using Domain_Layer.Entities;
using Domain_Layer.Abstract;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using Resturant_Ordering_System.Domain.Abstract;

namespace Business_Layer.Interfaces
{
    public interface IUow :  IDisposable
    {
        public IGenaricRepo<AppUser, string> AppUserRepo { get; }
        public ICartRepository Cart { get; }
        public ICatgoreyRepo Categories { get; }
        public IGenaricRepo<Favorite, int> Favorite { get; }
        public IFavoriteRepo FavoriteRepo { get; }
        public IMenuItemRepo MenuItems { get; }
        public ICouponRepo couponRepo { get; }
        public INotificationRepo Notifications { get; }
        public IGenaricRepo<OrderItems,int> OrderItems { get; }
        public IOrderRepo Orders { get; }
        public IReservationRepo Reservations { get; }
        public IReviewRepo Reviews { get; }
        public ITableRepo Tables { get; }
        public IGenaricRepo<Files,int> Files { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();

    }
}
