using LibRedminePower.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Exceptions
{
    public class RedmineConnectionException : ApplicationException
    {
        public RedmineConnectionException(string message, HttpStatusCode code, string url)
            : base(createMsg(string.Format(message, $"{(int)code} {code}", url)))
        {
        }

        public RedmineConnectionException(string message, string url, Exception ex)
            : base(createMsg(string.Format(message, url)), ex)
        {
        }

        public RedmineConnectionException(Exception ex)
            : base (createMsg(Resources.errRedmineConnectionError), ex)
        {
        }

        private static string createMsg(string error)
        {
            return string.Format(Resources.errConfirmRedmineSettings, error);
        }
    }
}
