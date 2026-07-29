using Data_Access_Layer.Data;
using Data_Access_Layer.Repositories;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class CouponRepo : MainGenaricRepo<Coupon,int> , ICouponRepo
    {
        private readonly DbSet<Coupon> dbset;
        public CouponRepo(AppDbContext context) : base(context) 
        {
            dbset = context.Set<Coupon>();
        }
        public IQueryable<Coupon> SearchCoupons(string searchKey)
        {
            return dbset.AsNoTracking().Where(x =>
             x.Code.Contains(searchKey.ToLower()));
        }
    }
}
