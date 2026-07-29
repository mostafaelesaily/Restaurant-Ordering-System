using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.TablesDTOs
{
    public class UpdateTablesDto
    {
        [Required]
        public int TableNumber { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string? QrCode { get; set; }
        public bool isActive { get; set; }
    }
}
