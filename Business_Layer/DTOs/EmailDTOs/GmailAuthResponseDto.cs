using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.DTOs.EmailDTOs
{
    public class GmailAuthResponseDto
    {
        public bool IsAuthorized { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
