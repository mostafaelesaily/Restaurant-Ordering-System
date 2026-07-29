using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.CatgoreyDTOs
{
    public class GetCatgoreyDto
    {
        public int id { get; set; }
        [MaxLength(100,ErrorMessage ="Name Has Max Value : (100) ")]
        [Required(ErrorMessage ="Name Filed is Required")]
        public string name { get; set; }
        [MaxLength(1000, ErrorMessage = "Name Has Max Value : (1000) ")]
        public string? description { get; set; }
    }
}
