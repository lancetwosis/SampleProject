using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Custom.Bases
{
    public abstract class CfBase<T> : FieldBase<T>
    {
        public CustomField Meta { get; set; }
        public IssueCustomField Cf { get; set; }

        public CfBase(CustomField meta, IssueCustomField cf, Func<T> getFunc, Action<T> setFunc) : base(meta.Name, getFunc, setFunc)
        {
            Meta = meta;
            Cf = cf;
        }
    }
}
