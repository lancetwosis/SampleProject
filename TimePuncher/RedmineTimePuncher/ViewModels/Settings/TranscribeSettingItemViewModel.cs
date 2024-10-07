using LibRedminePower.Extentions;
using NetOffice.OutlookApi;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class TranscribeSettingItemViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReadOnlyReactivePropertySlim<List<MyWikiPageItem>> WikiPages { get; set; }
        public ReadOnlyReactivePropertySlim<List<WikiLine>> Headers { get; set; }

        public ReactivePropertySlim<MyCustomFieldPossibleValue> Process { get; set; }
        public ReactivePropertySlim<MyProject> Project { get; set; }
        public ReactivePropertySlim<MyTracker> Tracker { get; set; }
        public ReactivePropertySlim<string> Title { get; set; }
        public ReactivePropertySlim<MyProject> WikiProject { get; set; }
        public ReactivePropertySlim<MyWikiPageItem> WikiPage { get; set; }
        public ReactivePropertySlim<bool> IncludesHeader { get; set; }
        public ReactivePropertySlim<WikiLine> Header { get; set; }

        public TranscribeSettingItemModel Model { get; set; }

        public TranscribeSettingItemViewModel() : this(new TranscribeSettingItemModel())
        { }

        public TranscribeSettingItemViewModel(TranscribeSettingItemModel model)
        {
            Model = model;

            Process = model.ToReactivePropertySlimAsSynchronized(a => a.Process).AddTo(disposables);
            Project = model.ToReactivePropertySlimAsSynchronized(a => a.Project).AddTo(disposables);
            Tracker = model.ToReactivePropertySlimAsSynchronized(a => a.Tracker).AddTo(disposables);
            Title = model.ToReactivePropertySlimAsSynchronized(a => a.Title).AddTo(disposables);
            WikiProject = model.ToReactivePropertySlimAsSynchronized(a => a.WikiProject).AddTo(disposables);
            WikiPage = model.ToReactivePropertySlimAsSynchronized(a => a.WikiPage).AddTo(disposables);
            IncludesHeader = model.ToReactivePropertySlimAsSynchronized(a => a.IncludesHeader).AddTo(disposables);
            Header = model.ToReactivePropertySlimAsSynchronized(a => a.Header).AddTo(disposables);

            WikiPages = WikiProject.CombineLatest(CacheTempManager.Default.Redmine, (p, redmine) =>
            {
                if (p == null || redmine == null)
                    return new List<MyWikiPageItem>();
                else
                    return redmine.GetAllWikiPages(p.Identifier);
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            WikiPages.SubscribeWithErr(pages =>
            {
                if (pages == null || !pages.Any())
                    WikiPage.Value = null;
                else
                    WikiPage.Value = pages.FirstOrFirst(w => w.Title == WikiPage.Value?.Title);
            }).AddTo(disposables);

            Headers = WikiPage.CombineLatest(CacheTempManager.Default.Redmine, (w, redmine) =>
            {
                var headers = new List<WikiLine>() { WikiLine.NOT_SPECIFIED };
                if (w == null || redmine == null)
                {
                    return headers;
                }
                else
                {
                    var wiki = redmine.GetWikiPage(w.ProjectId, w.Title);
                    headers.AddRange(wiki.GetHeaders(CacheTempManager.Default.MarkupLang.Value));
                    return headers;
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Headers.SubscribeWithErr(headers =>
            {
                Header.Value = headers.FirstOrFirst(h => h.Equals(Header.Value));
            }).AddTo(disposables);
        }
    }
}
