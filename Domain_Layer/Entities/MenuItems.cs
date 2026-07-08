using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class MenuItems
    {
        public int id { get; set; }

        public int categoryId { get; set; }

        public Categories categories { get; set; }

        public string name { get; set; }

        public string?  description { get; set; }

        public decimal price { get; set; }

        public string? ImageUrl { get; set; }

        public TimeSpan? PreparationTime { get; set; }

        public bool isAvailable { get; set; }

        public ICollection<CartItem> cartItems { get; set; } = new List<CartItem>();
        public ICollection<Favorite> favorites { get; set; } = new List<Favorite>();
        public ICollection<Reviews> reviews { get; set; } = new List<Reviews>();
        public ICollection<OrderItems> orderItems { get; set; } = new List<OrderItems>();
    }
}
