using LibRedminePower.Properties;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class AssignedTo : Bases.FieldList<IdentifiableName>
    {
        public AssignedTo(Issue issue, RedmineManager redmine)
            :base(
                 Resources.enumIssuePropertyTypeAssignedTo,
                 issue,
                 () => issue?.AssignedTo?.Id,
                 (v) => issue.AssignedTo = v.HasValue ? new IdentifiableName() { Id = v.Value } : null,
                 (v) => redmine.Users.SingleOrDefault(a => a.Id == v),
                 (v) => v?.Name)
        {
        }
    }
}
