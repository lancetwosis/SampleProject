using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard.Bases
{
    public abstract class FieldDate : FieldBase<DateTime?>
    {
        public FieldDate(string name, Func<DateTime?> getFunc, Action<DateTime?> setFunc) 
            : base(name, getFunc, setFunc)
        {
        }
    }
}
