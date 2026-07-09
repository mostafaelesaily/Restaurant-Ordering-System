using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
