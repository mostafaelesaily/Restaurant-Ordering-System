using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.CartDTOs
{
    public class AddToCartDto
    {
        [Required(ErrorMessage = "MenuItemId is required")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
