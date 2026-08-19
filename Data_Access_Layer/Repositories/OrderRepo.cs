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
    public class OrderRepo : MainGenaricRepo<Orders, int> ,IOrderRepo
    {
        private readonly DbSet<Orders> dbset;
        public OrderRepo(AppDbContext context) : base(context)
        {
            dbset = context.Set<Orders>();
        }

        public IQueryable<Orders> GetOrdersByCustomerId(string customerId)
        {
            return dbset
                .AsNoTracking()
                .Include(r => r.AppUser)

                .Where(r => r.customerId == customerId);
        }

        public IQueryable<Orders> SearchOrder(string searchKey)
        {
            return dbset
                .AsNoTracking()
                .Include(r => r.AppUser)
                .Include(r => r.Tables)
                .Include(r => r.Coupon)
                .Include(r => r.orderItems)
                .ThenInclude(oi => oi.menuItems)
                .Where(r =>
                    (r.AppUser != null &&
                    (
                        r.AppUser.UserName.Contains(searchKey) ||
                        r.AppUser.Email.Contains(searchKey) ||
                        r.AppUser.PhoneNumber.Contains(searchKey)
                    )) ||

                    (r.Coupon != null &&
                     r.Coupon.Code.Contains(searchKey)) ||

                    (r.Tables != null &&
                    (
                        r.Tables.TableNumber.ToString().Contains(searchKey) ||
                        r.Tables.Capacity.ToString().Contains(searchKey)
                    )) ||

                    r.orderItems.Any(oi =>
                        oi.menuItems != null &&
                        oi.menuItems.name.Contains(searchKey)
                    )
                );
        }
        public IQueryable<Orders> GetOrdersWithDetails()
        {
            return dbset
                .AsNoTracking()
                .Include(r => r.AppUser)
                .Include(r => r.Tables)
                .Include(r => r.Coupon)
                .Include(r => r.orderItems)
                .ThenInclude(r => r.menuItems);
        }
        public async Task<Orders?> GetOrderWithDetails(int orderId)
        {
            return await dbset
                .AsNoTracking()
                .Include(r => r.AppUser)
                .Include(r => r.Tables)
                .Include(r => r.Coupon)
                .Include(r => r.orderItems)
                .ThenInclude(r => r.menuItems)
                .FirstOrDefaultAsync(r => r.id == orderId);
        }
    }
}
