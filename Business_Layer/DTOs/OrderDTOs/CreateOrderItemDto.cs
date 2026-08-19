using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.OrderDTOs
{
    public class CreateOrderItemDto
    {
        public int MenuItemId { get; set; }

        public int Quantity { get; set; }
    }
}
