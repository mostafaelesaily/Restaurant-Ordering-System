using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    public class Files
    {
        public int id { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string FileType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? categoryId { get; set; }
        public Categories categories { get; set; }

        public int? menuItemId { get; set; }
        public MenuItems menuItems { get; set; }

        public int? reviewId { get; set; }
        public Reviews reviews { get; set; }
    }
}
