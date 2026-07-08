using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class OrderItems
    {
        public int id { get; set; }
        public int orderId { get; set; }

        public int menuItemId { get; set; }

        public int Quantity { get; set; }

        public decimal unitPrice { get; set; }

        public Orders order { get; set; }
        public MenuItems menuItems { get; set; }
    }
}
