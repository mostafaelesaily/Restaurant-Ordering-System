using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.ReviewDTOs
{
    public class ReviewDetailsDto
    {
        public int Id { get; set; }

        public int MenuItemId { get; set; }

        public string MenuItemName { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }
    }
}
