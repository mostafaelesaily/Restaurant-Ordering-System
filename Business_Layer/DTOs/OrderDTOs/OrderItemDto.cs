using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.OrderDTOs
{
    public class OrderItemDto
    {
        public string MenuItemName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
