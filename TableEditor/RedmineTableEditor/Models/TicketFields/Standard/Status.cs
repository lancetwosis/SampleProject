using LibRedminePower.Properties;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Standard.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class Status : FieldList<IssueStatus>
    {
        public Status(Issue issue, RedmineManager redmine) : base(
            Resources.enumIssuePropertyTypeStatus,
            issue,
            () => issue?.Status?.Id,
            (v) => issue.Status = new IdentifiableName() { Id = v.Value },
            (v) => redmine.Cache.Statuss.SingleOrDefault(a => a.Id == v),
            (v) => v?.Name)
        {
        }
    }
}
