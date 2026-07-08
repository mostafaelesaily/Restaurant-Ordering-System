using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }

        public int MenuItemId { get; set; }

        public int Quantity { get; set; }
        public Cart cart { get; set; }
        public MenuItems menuItems { get; set; }
    }
}
