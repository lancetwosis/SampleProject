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
    public class FixedVersion : FieldList<Redmine.Net.Api.Types.Version>
    {
        public FixedVersion(Issue issue, RedmineManager redmine) :
            base(
                Properties.Resources.enumIssuePropertyTypeFixedVersion,
                issue,
                () => issue?.FixedVersion?.Id,
                (v) => issue.FixedVersion = new IdentifiableName() { Id = v.Value },
                (v) => redmine.Versions.SingleOrDefault(a => a.Id == v),
                (v) => v?.Name)
        {
        }
    }
}
