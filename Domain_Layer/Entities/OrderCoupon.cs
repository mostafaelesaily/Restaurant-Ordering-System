using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class OrderCoupon
    {
        public int id {  get; set; }
        public int OrderId { get; set; }

        public int CouponId { get; set; }

        public Orders order { get; set; }
        public Coupon coupon { get; set; }
    }
}
