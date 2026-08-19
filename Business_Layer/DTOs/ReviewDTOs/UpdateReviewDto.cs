using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.ReviewDTOs
{
    public class UpdateReviewDto
    {
        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}
