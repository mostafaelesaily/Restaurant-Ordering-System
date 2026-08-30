using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.EmailDTOs
{
    public class SendEmailResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

    }
}
