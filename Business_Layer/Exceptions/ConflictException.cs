using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
