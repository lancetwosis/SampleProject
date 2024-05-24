using LibRedminePower.Properties;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class StartDate : Bases.FieldDate
    {
        public StartDate(Issue issue) :base(
            Resources.enumIssuePropertyTypeStartDate,
            () => issue?.StartDate,
            (v) => issue.StartDate = v)
        {
        }
    }
}
