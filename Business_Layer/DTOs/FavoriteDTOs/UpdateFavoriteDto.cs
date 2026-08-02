using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FavoriteDTOs
{
    public class UpdateFavoriteDto
    {
        [Required]
        public string CustomerId { get; set; }

        [Required]
        public int MenuItemId { get; set; }
    }
}
