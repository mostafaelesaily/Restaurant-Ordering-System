using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Reviews
    {
        public int Id { get; set; }

        public string CustomerId { get; set; }

        public int MenuItemId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public AppUser User { get; set; }
        public MenuItems MenuItems { get; set; }
    }
}
