using LibRedminePower.Properties;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class Tracker : Bases.FieldList<IdentifiableName>
    {
        public static Redmine.Net.Api.Types.Tracker NOT_SPECIFIED
            = new Redmine.Net.Api.Types.Tracker() { Id = 0, Name = Resources.SettingsNotSpecified };

        public Tracker(Issue issue, RedmineManager redmine)
            :base(
                 Resources.enumIssuePropertyTypeTracker,
                 issue,
                 () => issue?.Tracker?.Id,
                 (v) => issue.Tracker = new IdentifiableName() { Id = v.Value },
                 (v) => redmine.Trackers.SingleOrDefault(a => a.Id == v),
                 (v) => v?.Name)
        {
        }
    }
}
