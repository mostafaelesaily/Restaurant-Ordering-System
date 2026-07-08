using Domain_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Orders
    {
        public int id { get; set; }
        public string customerId { get; set; }
        public string? CheifId { get; set; }
        public string? DeliveryId { get; set; }

        public int ? TableId { get; set; }
        public OrderStatus Status { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ? UpdatedAt { get; set; }

        public DateTime EstimatedDeliveryTime { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string Address { get; set; }

        public string? Notes  { get; set;}

        public AppUser AppUser { get; set; }
        public AppUser Cheif { get; set; }
        public AppUser DeliveryUser { get;set; }
        public Tables Tables { get; set; }
        public ICollection<OrderCoupon> orderCoupons { get; set; } = new List<OrderCoupon>();
        public ICollection<OrderItems> orderItems { get; set; } = new List<OrderItems>();
    }
}
