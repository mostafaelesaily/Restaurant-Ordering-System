using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Categories
    {
        public int id { get; set; }
        public string name { get; set; }
        public string ? description { get; set; }

        public string ? ImageUrl { get; set; }

        public ICollection<MenuItems> items { get; set; } = new List<MenuItems>();
    }
}
