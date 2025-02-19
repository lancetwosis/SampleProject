using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket
{
    public class TranscribeSettingItemModel : LibRedminePower.Models.Bases.ModelBase
    {
        public MyProject Project { get; set; }
        public MyCustomFieldPossibleValue Process { get; set; }
        public MyTracker Tracker { get; set; }
        public string Title { get; set; }
        public MyProject WikiProject { get; set; }
        public MyWikiPageItem WikiPage { get; set; }
        public bool IncludesHeader { get; set; } = true;
        public WikiLine Header { get; set; }
        public bool ExpandsIncludeMacro { get; set; } = true;

        /// <summary>
        /// RadGridView の「Click here to add new item」機能のために引数なしのコンストラクタが必要となる
        /// </summary>
        public TranscribeSettingItemModel()
        { }

        public bool IsValid()
        {
            return Project != null && WikiProject != null && WikiPage != null && Header != null;
        }

        public bool NeedsTranscribe(MyIssue issue, MyCustomFieldPossibleValue process = null)
        {
            if (process != null)
                return Project.Id == issue.Project.Id &&
                       (Process == null || Process.Equals(TranscribeSettingModel.NOT_SPECIFIED_PROCESS) || Process.Equals(process)) &&
                       (Tracker == null || Tracker.Equals(MyTracker.NOT_SPECIFIED) || Tracker.Equals(issue.Tracker)) &&
                       (string.IsNullOrEmpty(Title) || Regex.IsMatch(issue.Subject, Title));
            else
                return Project.Id == issue.Project.Id &&
                       (Tracker == null || Tracker.Equals(MyTracker.NOT_SPECIFIED) || Tracker.Equals(issue.Tracker)) &&
                       (string.IsNullOrEmpty(Title) || Regex.IsMatch(issue.Subject, Title));
        }
    }
}
