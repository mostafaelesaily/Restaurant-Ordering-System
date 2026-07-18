using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.DTOs.UserDTOs
{
    public class UpdateUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
