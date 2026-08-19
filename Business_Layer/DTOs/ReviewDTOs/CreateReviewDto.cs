using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.ReviewDTOs
{
    public  class CreateReviewDto
    {
        public int MenuItemId { get; set; }
        [Range(1,5,ErrorMessage ="Rating Must Be Between 1 And 5")]
        public int Rating { get; set; }
        public string Comment { get; set; }

    }
}

