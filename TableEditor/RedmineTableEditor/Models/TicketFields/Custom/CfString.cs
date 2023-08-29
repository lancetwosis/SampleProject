using Redmine.Net.Api.Types;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.TicketFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Custom
{
    public class CfString : Bases.CfBase<string>
    {
        public CfString(CustomField meta, IssueCustomField cf)
            :base(meta,
                 cf,
                 () =>
                 {
                     if (cf.Values == null)
                         cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() };
                     return cf.Values[0]?.Info;
                 },
                 (v) =>
                 {
                     meta.Validate(v);
                     if (cf.Values == null)
                         cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() };
                     cf.Values[0].Info = v;
                })
        {
        }
    }
}
