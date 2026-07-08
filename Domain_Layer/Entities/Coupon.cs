using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public decimal Discount { get; set; }

        public DateTime ExpireDate { get; set; }

        public int MaxUsage { get; set; }

        public int CurrentUsage { get; set; }

        public bool IsActive { get; set; }

        public ICollection<OrderCoupon> orderCoupons { get; set; } = new List<OrderCoupon>();
    }
}
