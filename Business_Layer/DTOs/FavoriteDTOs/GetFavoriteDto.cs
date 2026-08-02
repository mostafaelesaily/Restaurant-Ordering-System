using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FavoriteDTOs
{
    public class GetFavoriteDto
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        public string MenuItemName { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public decimal Price { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
