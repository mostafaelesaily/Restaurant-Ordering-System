using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Business_Layer.DTOs.UserDTOs
{
    public class ChangePasswordDto
    {
        [Required]
        [MaxLength(255)]
        [MinLength(6)]

        public string oldPassword { get; set; }
        [Required]
        [MaxLength(255)]
        [MinLength(6)]
        public string newPassword { get; set; }

    }
}
