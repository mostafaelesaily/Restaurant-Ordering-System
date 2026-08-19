using Domain_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        public string Address { get; set; }
        public string? Notes { get; set; }
        public int? TableId { get; set; }
        public string? CouponCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<CreateOrderItemDto> itemDtos { get; set; } = new List<CreateOrderItemDto>();
    }
}
