using Business_Layer.Interfaces;
using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data_Access_Layer.UnitOfWork
{
    public class Uow : IUow
    {
        private readonly AppDbContext _context;

        public Uow(AppDbContext context)
        {
            _context = context;

            AppUserRepo = new MainGenaricRepo<AppUser, string>(_context);
            Cart = new MainGenaricRepo<Cart, int>(_context);
            CartItem = new MainGenaricRepo<CartItem, int>(_context);
            Categories = new catgoreyRepo(_context);
            couponRepo = new CouponRepo(_context);
            Favorite = new MainGenaricRepo<Favorite, int>(_context);
            MenuItems = new menuItemRepo(_context);
            Notifications = new MainGenaricRepo<Notifications, int>(_context);
            OrderCoupon = new MainGenaricRepo<OrderCoupon, int>(_context);
            OrderItems = new MainGenaricRepo<OrderItems, int>(_context);
            Orders = new MainGenaricRepo<Orders, int>(_context);
            Reservations = new MainGenaricRepo<Reservations, int>(_context);
            Reviews = new MainGenaricRepo<Reviews, int>(_context);
            Tables = new MainGenaricRepo<Tables, int>(_context);
            Files = new MainGenaricRepo<Files, int>(_context);
        }

        public IGenaricRepo<AppUser, string> AppUserRepo { get; }

        public IGenaricRepo<Cart, int> Cart { get; }

        public IGenaricRepo<CartItem, int> CartItem { get; }

        public ICatgoreyRepo Categories { get; }

        public ICouponRepo couponRepo {  get; }

        public IGenaricRepo<Favorite, int> Favorite { get; }

        public IMenuItemRepo MenuItems { get; }

        public IGenaricRepo<Notifications, int> Notifications { get; }

        public IGenaricRepo<OrderCoupon, int> OrderCoupon { get; }

        public IGenaricRepo<OrderItems, int> OrderItems { get; }

        public IGenaricRepo<Orders, int> Orders { get; }

        public IGenaricRepo<Reservations, int> Reservations { get; }

        public IGenaricRepo<Reviews, int> Reviews { get; }

        public IGenaricRepo<Tables, int> Tables { get; }

        public IGenaricRepo<Files, int> Files { get; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}