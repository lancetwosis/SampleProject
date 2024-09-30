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
        public List<MyCustomFieldPossibleValue> PossibleProcesses { get; set; }
        public MyProject Project { get; set; }
        public List<MyProject> PossibleProjects { get; set; }
        public MyTracker Tracker { get; set; }
        public List<MyTracker> PossibleTrackers { get; set; }
        public string Title { get; set; }
        public MyProject WikiProject { get; set; }
        public List<MyProject> PossibleWikiProjects { get; set; }
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
            var processes = new List<MyCustomFieldPossibleValue>() { TranscribeSettingModel.NOT_SPECIFIED_PROCESS };
            processes.AddRange(TranscribeSettingModel.PROCESSES);
            PossibleProcesses = processes;
            Process = PossibleProcesses.First();

            PossibleProjects = TranscribeSettingModel.PROJECTS;
            Project = PossibleProjects.FirstOrDefault();

            PossibleWikiProjects = TranscribeSettingModel.PROJECTS_ONLY_WIKI_ENABLED;
            WikiProject = PossibleWikiProjects.FirstOrDefault(p => p.Id == Project.Id);

            WikiPages = new List<MyWikiPageItem>();
            Headers = new List<WikiLine>();

            GoToWikiCommand = this.ObserveProperty(a => a.WikiPage).Select(w => w != null).ToReactiveCommand().WithSubscribe(() => WikiPage.GoToWiki()).AddTo(disposables);
        }

        public bool IsValid()
        {
            return Project != null && Process != null && WikiProject != null && WikiPage != null && Header != null;
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;

        public async Task SetupAsync(ReactivePropertySlim<string> isBusy)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            PossibleProjects = TranscribeSettingModel.PROJECTS;
            if (Project == null)
                Project = PossibleProjects.FirstOrDefault();

            PossibleWikiProjects = TranscribeSettingModel.PROJECTS_ONLY_WIKI_ENABLED;
            if (WikiProject == null)
                WikiProject = PossibleWikiProjects.FirstOrDefault(p => p.Id == Project.Id);

            await updateWikiPagesAsync(WikiProject, isBusy, () => WikiPages.FirstOrDefault(w => w.Title == WikiPage?.Title, WikiPages[0]));
            this.ObserveProperty(a => a.WikiProject).Skip(1).SubscribeWithErr(async p =>
            {
                await updateWikiPagesAsync(p, isBusy, () => WikiPages.FirstOrDefault(w => w.IsTopWiki, WikiPages[0]));
            }).AddTo(myDisposables);

            var myTrackers = new List<MyTracker>() { MyTracker.NOT_SPECIFIED };
            myTrackers.AddRange(CacheManager.Default.TmpTrackers.Select(t => new MyTracker(t)));
            PossibleTrackers = myTrackers;
            Tracker = PossibleTrackers.FirstOrDefault(t => t.Equals(Tracker), PossibleTrackers[0]);

            await updateHeadersAsync(WikiPage, isBusy, () => Headers.FirstOrDefault(h => h.Equals(Header), Headers[0]));
            this.ObserveProperty(a => a.WikiPage).Skip(1).SubscribeWithErr(async w =>
            {
                await updateHeadersAsync(w, isBusy, () => Headers.First());
            }).AddTo(myDisposables);
        }

        private async Task updateWikiPagesAsync(MyProject p, ReactivePropertySlim<string> isBusy, Func<MyWikiPageItem> selector)
        {
            try
            {
                if (p == null)
                {
                    WikiPages = new List<MyWikiPageItem>();
                    WikiPage = null;
                    return;
                }

                isBusy.Value = Resources.SettingsMsgNowGettingData;

                await Task.Run(() =>
                {
                    // 選択されたプロジェクトで Wiki がモジュールとして有効になっていても
                    // 一切 Wiki の編集を行っていない場合、WikiPages が空になるため、以下の処理とする
                    WikiPages = TranscribeSettingModel.REDMINE.GetAllWikiPages(p.Identifier);
                    if (WikiPages.Count > 0)
                        WikiPage = selector.Invoke();
                    else
                        WikiPage = null;
                });
            }
            finally
            {
                isBusy.Value = null;
            }
;        }

        private async Task updateHeadersAsync(MyWikiPageItem w, ReactivePropertySlim<string> isBusy, Func<WikiLine> selector)
        {
            try
            {
                if (w == null)
                {
                    Headers = new List<WikiLine>() { WikiLine.NOT_SPECIFIED };
                    Header = Headers[0];
                    return;
                }

                isBusy.Value = Resources.SettingsMsgNowGettingData;

                await Task.Run(() =>
                {
                    // GetAllWikiPages で取得すると Text が null のため再取得する
                    var wiki = TranscribeSettingModel.REDMINE.GetWikiPage(w.ProjectId, w.Title);

                    var headers = new List<WikiLine>() { WikiLine.NOT_SPECIFIED };
                    headers.AddRange(wiki.GetHeaders(CacheManager.Default.TmpMarkupLang));

                    Headers = headers;
                    Header = selector.Invoke();
                });
            }
            finally
            {
                isBusy.Value = null;
            }
        }

        public string ResetProcess()
        {
            var processes = new List<MyCustomFieldPossibleValue>() { TranscribeSettingModel.NOT_SPECIFIED_PROCESS };
            processes.AddRange(TranscribeSettingModel.PROCESSES);
            PossibleProcesses = processes;

            var preValue = Process;
            var proc = PossibleProcesses.FirstOrDefault(p => p.Equals(preValue));
            Process = proc != null ? proc : PossibleProcesses[0];
            return proc != null ? null : string.Format(Resources.SettingsReviErrMsgFailedSetProcess, preValue.Label);
        }

        public string ClearProcess()
        {
            PossibleProcesses = new List<MyCustomFieldPossibleValue>() { TranscribeSettingModel.NOT_SPECIFIED_PROCESS };

            var preValue = Process;
            Process = PossibleProcesses[0];
            return preValue.Equals(Process) ? null : string.Format(Resources.SettingsReviErrMsgFailedSetProcess, preValue.Label);
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
