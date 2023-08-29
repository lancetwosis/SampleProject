using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class DueDate : Bases.FieldDate
    {
        public DueDate(Issue issue) : base(
            Properties.Resources.enumIssuePropertyTypeDueDate,
            () => issue?.DueDate,
            (v) => issue.DueDate = v)
        {
        }
    }
}
