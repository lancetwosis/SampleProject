using LibRedminePower.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Exceptions
{
    public class RedmineLoginFailedException : ApplicationException
    {
        public RedmineLoginFailedException() : base(Resources.errLoginFailed)
        {
        }
    }
}
