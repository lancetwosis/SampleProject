using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class DoneRatio : Bases.FieldDouble
    {
        public DoneRatio(Issue issue) : base(
            Properties.Resources.enumIssuePropertyTypeDoneRatio,
            () =>
            {
                if (issue == null) return null;
                if (issue.DoneRatio == null) return null;
                var myDeci = (decimal)issue?.DoneRatio;
                return decimal.ToDouble(myDeci);
            },
            (v) =>
            {
                if (v.HasValue)
                    issue.DoneRatio = (float)v.Value;
            })
        {
        }
    }
}
