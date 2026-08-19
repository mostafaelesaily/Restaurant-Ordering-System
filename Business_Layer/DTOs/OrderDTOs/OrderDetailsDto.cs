using Domain_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.OrderDTOs
{
    public class OrderDetailsDto
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; }

        public string? ChefName { get; set; }

        public string? DeliveryName { get; set; }

        public int? TableNumber { get; set; }

        public string? CouponCode { get; set; }

        public OrderStatus Status { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string? Address { get; set; }

        public string? Notes { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<OrderItemDto> Items { get; set; } = [];
    }
}
