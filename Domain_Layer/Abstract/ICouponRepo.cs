using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Abstract
{
    public interface ICouponRepo : IGenaricRepo<Coupon,int>
    {
        IQueryable<Coupon> SearchCoupons(string searchKey);
    }
}
