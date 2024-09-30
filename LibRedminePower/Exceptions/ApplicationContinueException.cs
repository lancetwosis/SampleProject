using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Exceptions
{
    public class ApplicationContinueException : ApplicationException
    {
        public ApplicationContinueException(string message, Exception e) : base(message, e)
        {
        }
    }
}
