using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.MenuItemDTOs
{
    public class CreateMenuItemDto
    {
        [Required(ErrorMessage = "Name Filed is Required")]
        [MaxLength(100, ErrorMessage = "Name Has Max Value : (100) ")]
        public string name { get; set; }
        [MaxLength(1000, ErrorMessage = "Description Has Max Value : (1000) ")]
        public string? description { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public decimal price { get; set; }
        public TimeSpan? PreparationTime { get; set; }
        public bool isAvailable { get; set; } = true;
        [Required(ErrorMessage = "CategoryId is required")]
        public int categoryId { get; set; }
    }
}
