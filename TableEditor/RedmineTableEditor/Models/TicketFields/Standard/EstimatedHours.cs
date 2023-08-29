using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class EstimatedHours : Bases.FieldDouble
    {
        public EstimatedHours(Issue issue) :base(
            Properties.Resources.enumIssuePropertyTypeEstimatedHours,
            () =>
            {
                if (issue == null) return null;
                if (issue.EstimatedHours == null) return null;

                var myDeci = (decimal)issue?.EstimatedHours;
                var result = decimal.ToDouble(myDeci);
                return result;
            },
            (v) =>
            {
                if (v.HasValue)
                    issue.EstimatedHours = (float)v;
            })
        {
        }
    }
}
