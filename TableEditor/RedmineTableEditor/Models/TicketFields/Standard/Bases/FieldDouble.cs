using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard.Bases
{
    public abstract class FieldDouble : FieldBase<double?>
    {
        public FieldDouble(string name, Func<double?> getFunc, Action<double?> setFunc)
            : base(name, getFunc, setFunc)
        {
        }
    }
}
