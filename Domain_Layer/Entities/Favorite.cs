using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Favorite
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }

        public int MenuItemId { get; set; }

        public MenuItems MenuItems { get; set; }
        public AppUser User { get; set; }
    }
}
