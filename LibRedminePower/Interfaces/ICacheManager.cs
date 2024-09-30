using LibRedminePower.Enums;
using LibRedminePower.Models;
using Reactive.Bindings;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Interfaces
{
    public interface ICacheManager
    {
        ReadOnlyReactivePropertySlim<DateTime> Updated { get; set; }

        List<Project> Projects { get; set; }
        List<Tracker> Trackers { get; set; }
        List<IssueStatus> Statuss { get; set; }
        List<IssuePriority> Priorities { get; set; }
        List<TimeEntryActivity> TimeEntryActivities { get; set; }
        List<Query> Queries { get; set; }
        List<CustomField> CustomFields { get; set; }
        Dictionary<int, List<Redmine.Net.Api.Types.Version>> ProjectVersions { get; set; }
        Dictionary<int, List<ProjectMembership>> ProjectMemberships { get; set; }
        MarkupLangType MarkupLang { get; set; }
        MyUser MyUser { get; set; }
        List<MyUser> Users { get; set; }

        void ForceUpdate();
    }
}
