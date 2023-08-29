using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Exceptions
{
    public class ExpiredException : ApplicationException
    {
        public ExpiredException() : base(Properties.Resources.msgErrExceedsLimitDate)
        {
        }
    }
}
