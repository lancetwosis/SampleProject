using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class Priority : Bases.FieldList<IssuePriority>
    {
        public Priority(Issue issue, RedmineManager redmine) :
            base(
                Properties.Resources.enumIssuePropertyTypePriority,
                issue,
                () => issue?.Priority?.Id,
                (v) => issue.Priority = new IdentifiableName() { Id = v.Value},
                (v) => redmine.Priorities.SingleOrDefault(a => a.Id == v),
                (v) => v?.Name)
        {
        }
    }
}
