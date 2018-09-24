using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Helpers
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message):base(message)
        {
            
        }
    }
}
