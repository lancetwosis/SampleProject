using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard.Bases
{
    public abstract class FieldText : FieldBase<string>
    {
        public FieldText(string name, Func<string> getFunc, Action<string> setFunc) 
            : base(name, getFunc, setFunc)
        {
        }
    }
}
