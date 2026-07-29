using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.TablesDTOs
{
    public class CreateTablesDto
    {
        [Required]
        public int TableNumber { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string? QrCode { get; set; }
    }
}
