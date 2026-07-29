using Domain_Layer.Entities;
using Domain_Layer.Abstract;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Interfaces
{
    public interface IUow :  IDisposable
    {
        public IGenaricRepo<AppUser, string> AppUserRepo { get; }
        public IGenaricRepo<Cart,int> Cart { get; }
        public IGenaricRepo<CartItem,int> CartItem { get; }
        public ICatgoreyRepo Categories { get; }
        public IGenaricRepo<Favorite, int> Favorite { get; }
        public IMenuItemRepo MenuItems { get; }
        public ICouponRepo couponRepo { get; }
        public IGenaricRepo<Notifications,int> Notifications { get; }
        public IGenaricRepo<OrderCoupon, int> OrderCoupon { get; }
        public IGenaricRepo<OrderItems,int> OrderItems { get; }
        public IGenaricRepo<Orders,int> Orders { get; }
        public IGenaricRepo<Reservations,int> Reservations { get; }
        public IGenaricRepo<Reviews,int> Reviews { get; }
        public IGenaricRepo<Tables,int> Tables { get; }
        public IGenaricRepo<Files,int> Files { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();

    }
}
