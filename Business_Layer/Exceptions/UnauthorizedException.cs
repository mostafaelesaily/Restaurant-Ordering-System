using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
