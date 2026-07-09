using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
