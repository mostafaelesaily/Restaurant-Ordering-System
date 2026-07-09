using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data_Access_Layer.UnitOfWork
{
    public class uow : IUow
    {
        private readonly AppDbContext _context;
        public uow(AppDbContext context)
        {
            _context = context;
            AppUserRepo = new MainGenaricRepo<AppUser, string>(_context);
            Cart = new MainGenaricRepo<Cart, int>(_context);
            CartItem = new MainGenaricRepo<CartItem, int>(_context);
            Categories = new MainGenaricRepo<Categories, int>(_context);
            Coupon = new MainGenaricRepo<Coupon, int>(_context);
            Favorite = new MainGenaricRepo<Favorite, int>(_context);
            MenuItems = new MainGenaricRepo<MenuItems, int>(_context);
            Notifications = new MainGenaricRepo<Notifications, int>(_context);
            OrderCoupon = new MainGenaricRepo<OrderCoupon, int>(_context);
            OrderItems = new MainGenaricRepo<OrderItems, int>(_context);
            Orders = new MainGenaricRepo<Orders, int>(_context);
            Reservations = new MainGenaricRepo<Reservations, int>(_context);
            Reviews = new MainGenaricRepo<Reviews, int>(_context);
            Tables = new MainGenaricRepo<Tables, int>(_context);
        }
        public IGenaricRepo<AppUser, string> AppUserRepo { get; private set; }

        public IGenaricRepo<Cart, int> Cart { get; private set; }

        public IGenaricRepo<CartItem, int> CartItem { get; private set; }

        public IGenaricRepo<Categories, int> Categories { get; private set; }   

        public IGenaricRepo<Coupon, int> Coupon { get; private set; }

        public IGenaricRepo<Favorite, int> Favorite { get; private set; }

        public IGenaricRepo<MenuItems, int> MenuItems { get; private set; }

        public IGenaricRepo<Notifications, int> Notifications { get; private set; }

        public IGenaricRepo<OrderCoupon, int> OrderCoupon { get; private set; }

        public IGenaricRepo<OrderItems, int> OrderItems { get; private set; }

        public IGenaricRepo<Orders, int> Orders { get; private set; }

        public IGenaricRepo<Reservations, int> Reservations { get; private set; }   

        public IGenaricRepo<Reviews, int> Reviews { get; private set; }

        public IGenaricRepo<Tables, int> Tables { get; private set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public Task<int> SaveChanges()
        {
            return _context.SaveChangesAsync();
        }
    }
}
