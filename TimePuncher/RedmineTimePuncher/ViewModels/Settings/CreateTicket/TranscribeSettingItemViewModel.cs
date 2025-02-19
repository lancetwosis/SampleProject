using LibRedminePower.Extentions;
using NetOffice.OutlookApi;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace RedmineTimePuncher.ViewModels.Settings.CreateTicket
{
    public class TranscribeSettingItemViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReadOnlyReactivePropertySlim<List<MyWikiPageItem>> WikiPages { get; set; }
        public ReadOnlyReactivePropertySlim<List<WikiLine>> Headers { get; set; }

        public ReactivePropertySlim<MyProject> Project { get; set; }
        public ReactivePropertySlim<MyCustomFieldPossibleValue> Process { get; set; }
        public ReactivePropertySlim<MyTracker> Tracker { get; set; }
        public ReactivePropertySlim<string> Title { get; set; }
        public ReactivePropertySlim<MyProject> WikiProject { get; set; }
        public ReactivePropertySlim<MyWikiPageItem> WikiPage { get; set; }
        public ReactivePropertySlim<bool> IncludesHeader { get; set; }
        public ReactivePropertySlim<WikiLine> Header { get; set; }
        public ReactivePropertySlim<bool> ExpandsIncludeMacro { get; set; }

        public TranscribeSettingItemModel Model { get; set; }

        public TranscribeSettingItemViewModel() : this(new TranscribeSettingItemModel())
        { }

        public TranscribeSettingItemViewModel(TranscribeSettingItemModel model)
        {
            Model = model;

            Project = model.ToReactivePropertySlimAsSynchronized(a => a.Project).AddTo(disposables);
            Process = model.ToReactivePropertySlimAsSynchronized(a => a.Process).AddTo(disposables);
            Tracker = model.ToReactivePropertySlimAsSynchronized(a => a.Tracker).AddTo(disposables);
            Title = model.ToReactivePropertySlimAsSynchronized(a => a.Title).AddTo(disposables);
            WikiProject = model.ToReactivePropertySlimAsSynchronized(a => a.WikiProject).AddTo(disposables);
            WikiPage = model.ToReactivePropertySlimAsSynchronized(a => a.WikiPage).AddTo(disposables);
            IncludesHeader = model.ToReactivePropertySlimAsSynchronized(a => a.IncludesHeader).AddTo(disposables);
            Header = model.ToReactivePropertySlimAsSynchronized(a => a.Header).AddTo(disposables);
            ExpandsIncludeMacro = model.ToReactivePropertySlimAsSynchronized(a => a.ExpandsIncludeMacro).AddTo(disposables);

            WikiPages = WikiProject.CombineLatest(CacheTempManager.Default.Redmine, (p, redmine) =>
            {
                if (redmine == null)
                    return null;
                else
                    return p != null ? redmine.GetAllWikiPages(p.Identifier) : new List<MyWikiPageItem>();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            WikiPages.Where(a => a != null).SubscribeWithErr(pages =>
            {
                WikiPage.Value = pages.FirstOrFirst(w => w.Title == WikiPage.Value?.Title);
            }).AddTo(disposables);

            Headers = WikiPage.CombineLatest(CacheTempManager.Default.Redmine, (w, redmine) =>
            {
                if (redmine == null)
                    return null;

                var headers = new List<WikiLine>() { WikiLine.NOT_SPECIFIED };
                if (w == null)
                    return headers;

                var wiki = redmine.GetWikiPage(w.ProjectId, w.Title);
                headers.AddRange(wiki.GetHeaders(CacheTempManager.Default.MarkupLang.Value));
                return headers;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Headers.Where(a => a != null).SubscribeWithErr(headers =>
            {
                Header.Value = headers.FirstOrFirst(h => h.Equals(Header.Value));
            }).AddTo(disposables);
        }
    }
}
