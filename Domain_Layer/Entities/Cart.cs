using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public AppUser User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
