using LibRedminePower.Enums;
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
        ReactiveProperty<List<Project>> Projects { get; set; }
        ReactiveProperty<List<Tracker>> Trackers { get; set; }
        ReactiveProperty<List<IssueStatus>> Statuss { get; set; }
        ReactiveProperty<List<IssuePriority>> Priorities { get; set; }
        ReactiveProperty<List<TimeEntryActivity>> TimeEntryActivities { get; set; }
        ReactiveProperty<List<Query>> Queries { get; set; }
        ReactiveProperty<List<CustomField>> CustomFields { get; set; }
        ReactiveProperty<MarkupLangType> MarkupLang { get; set; }
    }
}
