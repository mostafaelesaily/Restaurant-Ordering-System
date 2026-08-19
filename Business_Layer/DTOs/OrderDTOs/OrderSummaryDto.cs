using Domain_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.OrderDTOs
{
    public class OrderSummaryDto
    {
        public int orderId { get; set; }
        public string customerName { get; set; }
        public OrderStatus status { get; set; }
        public decimal totalPrice { get; set; }
        public PaymentStatus paymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
