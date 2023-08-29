using Redmine.Net.Api.Types;
using RedmineTableEditor.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Custom
{
    public class CfInt : Bases.CfBase<int?>
    {
        public CfInt(CustomField meta, IssueCustomField cf)
            : base(meta,
                  cf,
                  () =>
                  {
                      if (cf.Values == null)
                          cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() };
                      if (int.TryParse(cf.Values[0].Info, out var result))
                          return result;
                      return null;
                  },
                  (v) =>
                  {
                      meta.Validate(v.ToString());
                      if (cf.Values == null)
                          cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() };
                      cf.Values[0].Info = v.ToString();                
                  })
        {
        }
    }
}
