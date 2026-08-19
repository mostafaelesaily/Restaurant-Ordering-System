using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.OrderDTOs
{
    public class CreateOrderByAdminDto : CreateOrderDto
    {
        public string? CheifId { get; set; }
        public string? DeliveryId { get; set; }
        public string CustomerId { get; set; }
    }
}
