using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.CatgoreyDTOs
{
    public class CreateCatgoreyDto
    {
        [Required(ErrorMessage = "Name Filed is Required")]
        [MaxLength(100, ErrorMessage = "Name Has Max Value : (100) ")]
        public string name { get; set; }
        [MaxLength(1000, ErrorMessage = "Name Has Max Value : (1000) ")]
        public string? description { get; set; }
    }
}
