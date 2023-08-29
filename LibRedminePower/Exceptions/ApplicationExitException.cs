using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Exceptions
{
    public class ApplicationExitException : ApplicationException
    {
        public ApplicationExitException(string message) : base(message)
        {}
    }
}
