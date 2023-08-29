using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class Category : Bases.FieldList<IssueCategory>
    {
        public Category(Issue issue, RedmineManager redmine) : base(
            Properties.Resources.enumIssuePropertyTypeCategory,
            issue,
            () => issue?.Category?.Id,
            (v) => issue.Category = new IdentifiableName() { Id = v.Value },
            (v) => redmine.Categories?.SingleOrDefault(a => a.Id == v),
            (v) => v?.Name)
        {
        }
    }
}
