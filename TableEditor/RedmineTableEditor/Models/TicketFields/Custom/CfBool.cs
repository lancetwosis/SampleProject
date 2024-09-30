using Redmine.Net.Api.Types;
using RedmineTableEditor.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Custom
{
    public class CfBool : Bases.CfBase<bool?>
    {
        public CfBool(CustomField meta, IssueCustomField cf)
            : base(meta, 
                  cf,
                  () =>
                  {
                      if (cf.Values == null)
                          cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() };
                      switch(cf.Values[0].Info)
                      {
                          case "0": return false;
                          case "1": return true;
                          default:  return null;
                      }
                  },
                  (v) =>
                  {
                      meta.Validate(v.ToString());
                      if (cf.Values == null)
                          cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() };
                      if (v.HasValue)
                          cf.Values[0].Info = v.Value ? "1" : "0";
                      else
                          cf.Values[0].Info = null;
                  })
        {
        }
    }
}
