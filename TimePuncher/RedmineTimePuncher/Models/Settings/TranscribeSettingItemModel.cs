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

namespace RedmineTimePuncher.Models.Settings
{
    public class TranscribeSettingItemModel : LibRedminePower.Models.Bases.ModelBase
    {
        public MyCustomFieldPossibleValue Process { get; set; }
        public MyProject Project { get; set; }
        public MyTracker Tracker { get; set; }
        public string Title { get; set; }
        public MyProject WikiProject { get; set; }
        public MyWikiPageItem WikiPage { get; set; }
        public List<MyWikiPageItem> WikiPages { get; set; }
        public bool IncludesHeader { get; set; } = true;
        public WikiLine Header { get; set; }
        public List<WikiLine> Headers { get; set; }

        [JsonIgnore]
        public ReactiveCommand GoToWikiCommand { get; set; }

        /// <summary>
        /// RadGridView の「Click here to add new item」機能のために引数なしのコンストラクタが必要となる
        /// よって、ReviewTranscribeSettingsModel にて static で有効な Processes と Projects を保持し、それを利用してインスタンスを生成する
        /// </summary>
        public TranscribeSettingItemModel()
        {
            WikiPages = new List<MyWikiPageItem>();
            Headers = new List<WikiLine>();

            GoToWikiCommand = this.ObserveProperty(a => a.WikiPage).Select(w => w != null).ToReactiveCommand().WithSubscribe(() => WikiPage.GoToWiki()).AddTo(disposables);
        }

        public bool IsValid()
        {
            return Project != null && Process != null && WikiProject != null && WikiPage != null && Header != null;
        }

        public bool NeedsTranscribe(MyIssue issue, MyCustomFieldPossibleValue process = null)
        {
            if (process != null)
                return Project.Id == issue.Project.Id &&
                       (Process.Equals(TranscribeSettingModel.NOT_SPECIFIED_PROCESS) || Process.Equals(process)) &&
                       (Tracker.Equals(MyTracker.NOT_SPECIFIED) || Tracker.Equals(issue.Tracker)) &&
                       (string.IsNullOrEmpty(Title) || Regex.IsMatch(issue.Subject, Title));
            else
                return Project.Id == issue.Project.Id &&
                       (Tracker.Equals(MyTracker.NOT_SPECIFIED) || Tracker.Equals(issue.Tracker)) &&
                       (string.IsNullOrEmpty(Title) || Regex.IsMatch(issue.Subject, Title));
        }
    }
}
