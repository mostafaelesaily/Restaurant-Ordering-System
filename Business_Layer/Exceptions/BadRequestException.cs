using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        
        }
    }
}
