using Business_Layer.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.UserDTOs
{
    public class AddEmployeeResponseDto
    {
        public GetUserDto getUserDto;
        public string TemporaryPassword { get; set; }

    }
}
