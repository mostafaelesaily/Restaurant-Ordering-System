using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.DTOs.Authentication
{
    public class AuthenticationResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireOn { get; set; }
        public string message { get; set; }

    }
}
