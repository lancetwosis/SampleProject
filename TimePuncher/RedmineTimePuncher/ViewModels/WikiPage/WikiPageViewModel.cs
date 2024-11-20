using LibRedminePower;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using RedmineTimePuncher.Enums;
using Redmine.Net.Api;
using LibRedminePower.Extentions;
using System.Data;
using static NetOffice.OfficeApi.Tools.Contribution.DialogUtils;
using System.Runtime.InteropServices;
using NetOffice.OutlookApi;
using RedmineTimePuncher.ViewModels.WikiPage.Charts;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using LibRedminePower.Helpers;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel.Composition.Primitives;

namespace RedmineTimePuncher.ViewModels.WikiPage
{
    public class WikiPageViewModel : FunctionViewModelBase
    {
        public BusyNotifier IsBusy { get; set; }
        public ObservableCollection<MyProject> Projects { get; set; }
        public ReactivePropertySlim<MyProject> SelectedProject { get; set; }
        public AsyncCommandBase ReloadCommand { get; }
        public AsyncCommandBase ExportCommand { get; set; }

        public ObservableCollection<MyWikiPageCount> WikiPages { get; set; }
        public ObservableCollection<MyWikiPageCount> SelectedWikiPages { get; set; }
        public ReadOnlyReactivePropertySlim<ReadOnlyReactivePropertySlim<bool>> IsBusyUpdateHistories { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyWikiHistorySummary>> HistorySummaries { get; set; }
        public ObservableCollection<MyWikiHistorySummary> SelectedHistorySummaries { get; set; }
        public AsyncCommandBase DiffHistoriesCommand { get; set; }
        public ReadOnlyReactivePropertySlim<List<WikiUserSummary>> UserSummaries { get; set; }
        public ReadOnlyReactivePropertySlim<List<SeriesViewModelBase>> Serieses { get; set;}
        public ReactivePropertySlim<Enums.WikiPeriodType> SelectedPeriodType { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsHistories { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsPeriodNumericType { get; set; }
        public ReactivePropertySlim<int> SelectedPeriodNumeric { get; set; }
        public ReactivePropertySlim<DateTime> SelectedStartDate { get; set; }
        public ReactivePropertySlim<DateTime> SelectedEndDate { get; set; }

        public WikiPageViewModel() : base(ApplicationMode.WikiPage)
        {
            IsBusy = new BusyNotifier();

            Projects = new ObservableCollection<MyProject>();
            SelectedProject = new ReactivePropertySlim<MyProject>().AddTo(disposables);

            WikiPages = new ObservableCollection<MyWikiPageCount>();
            SelectedWikiPages = new ObservableCollection<MyWikiPageCount>();

            SelectedPeriodType = new ReactivePropertySlim<WikiPeriodType>().AddTo(disposables);
            IsPeriodNumericType = SelectedPeriodType.Select(a => a == WikiPeriodType.LastNMonth || a == WikiPeriodType.LastNWeeks).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectedPeriodNumeric = new ReactivePropertySlim<int>(1);
            SelectedStartDate = new ReactivePropertySlim<DateTime>(DateTime.Today.AddDays(-7));
            SelectedEndDate = new ReactivePropertySlim<DateTime>(DateTime.Today);

            var selectedPeriod = SelectedPeriodType.CombineLatest(SelectedPeriodNumeric, SelectedStartDate, SelectedEndDate)
                .Select(a => a.First.GetPeriod(a.Second, a.Third, a.Fourth)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectedPeriodType.SubscribeWithErr(_ =>
                SelectedWikiPages.ToList().ForEach(async wiki => await wiki.UpdateHistoriesAsync(Models.Managers.RedmineManager.Default.Value, selectedPeriod.Value))).AddTo(disposables);
            SelectedWikiPages.ObserveAddChanged().SubscribeWithErr(async wiki =>
                await wiki.UpdateHistoriesAsync(Models.Managers.RedmineManager.Default.Value, selectedPeriod.Value)).AddTo(disposables);
            SelectedWikiPages.ObserveRemoveChanged().SubscribeWithErr(wiki => wiki.CtsUpdateHistories?.Cancel()).AddTo(disposables);
            SelectedWikiPages.ObserveResetChanged().SubscribeWithErr(_ => SelectedWikiPages.ToList().ForEach(a  => a.CtsUpdateHistories?.Cancel())).AddTo(disposables);

            IsHistories = SelectedPeriodType.Select(a => a == WikiPeriodType.None).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsBusyUpdateHistories = 
                SelectedWikiPages.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                {
                    if (SelectedWikiPages.Any())
                        return SelectedWikiPages.Select(a => a.IsBusyUpdateHistories).CombineLatestValuesAreAllFalse().Inverse().ToReadOnlyReactivePropertySlim();
                    else
                        return new BusyNotifier().ToReadOnlyReactivePropertySlim(false);
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectedHistorySummaries = new ObservableCollection<MyWikiHistorySummary>();

            DiffHistoriesCommand = new AsyncCommandBase(
                Properties.Resources.DiffHistories, 
                Properties.Resources.icons8_compare_16,
                SelectedHistorySummaries.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                        SelectedHistorySummaries.Select(a => a.Title).Distinct().Count() == 1 && SelectedHistorySummaries.Count >= 2).Select(a => !a ? "" : null),
                async () =>
                {
                    var temp = SelectedHistorySummaries.OrderBy(a => a.UpdatedOn);
                    var _new = temp.First();
                    var _old = temp.Last();
                    var url = $"{_new.Url}/diff?version={_new.Version}&version_from={_old.Version}";
                    Process.Start(url);
                }).AddTo(disposables);

            HistorySummaries =
                IsHistories.CombineLatest(IsBusyUpdateHistories.ObserveProperty(a => a.Value.Value)).Select(a =>
            {
                if (!a.First)
                {
                    if (!a.Second)
                    {
                        return SelectedWikiPages.SelectMany(h => h.Histories).OrderByDescending(b => b.UpdatedOn).ToList();
                    }
                }
                return new List<MyWikiHistorySummary>();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            UserSummaries = HistorySummaries.Select(allhs => 
            {
                return allhs.GroupBy(h => h.Author).Select(x => new WikiUserSummary()
                {
                    Author = x.Key,
                    InsertNoOfChar = x.Sum(y => y.InsertNoOfChar),
                    InsertNoOfLine = x.Sum(y => y.InsertNoOfLine),
                    DeleteNoOfChar = x.Sum(y => y.DeleteNoOfChar),
                    DeleteNoOfLine = x.Sum(y => y.DeleteNoOfLine),
                }).OrderByDescending(a => a.InsertNoOfLine).ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Serieses = HistorySummaries.Select(allhs =>
            {
                // 各テーブル毎の行数シリーズを作成する。
                var wikiSerieis = allhs.GroupBy(a => a.Title).Select(a =>
                {
                    var series = new WikiSeriesViewModel();
                    series.Title = a.Key;
                    series.Items = 
                        a.GroupBy(b => b.UpdatedOn.Value.Date)
                        .Select(b => new DataItem() { XValue = b.Key, YValue = b.First().Summary.NoOfLine })
                        .OrderBy(b => b.XValue).ToList();
                    return series;
                }).ToList();

                // ユーザー毎の追加行数シリーズを作成する。
                var userSerieis = allhs.GroupBy(h => h.Author).Select(a =>
                {
                    var series = new UserSeriesViewModel();
                    series.Title = a.Key.Name;
                    series.Items =
                        a.GroupBy(b => b.UpdatedOn.Value.Date)
                        .Select(b => new DataItem() { XValue = b.Key, YValue = b.Sum(c => c.InsertNoOfLine) })
                        .OrderBy(b => b.XValue).ToList();
                    return series;
                }).ToList();
                // 追加行数を累積行数に変換する。
                foreach (var userSeries in userSerieis)
                {
                    var cumVal = 0;
                    foreach (var item in userSeries.Items)
                    {
                        cumVal += item.YValue;
                        item.YValue = cumVal;
                    }
                }

                // X軸をそろえるため、全データのX軸の日付をリストアップする。
                var allSeries = wikiSerieis.Cast<SeriesViewModelBase>().Concat(userSerieis.Cast<SeriesViewModelBase>()).ToList();
                var allDates = allSeries.SelectMany(a => a.Items.Select(b => b.XValue)).Distinct().OrderBy(a => a).ToList();
                foreach (var date in allDates)
                {
                    foreach (var page in allSeries)
                    {
                        // 日付が設定されていなかった場合
                        if (!page.Items.Any(a => a.XValue == date))
                        {
                            // 前までの日の最終データで保管する。
                            var prevValue = page.Items.Where(a => a.XValue < date).OrderBy(a => a.XValue).LastOrDefault();
                            var index = prevValue == null ? 0 : (page.Items.IndexOf(prevValue) + 1);
                            if (index <= 0) index = 0;
                            page.Items.Insert(
                                index,
                                new DataItem()
                                {
                                    XValue = date,
                                    YValue = prevValue != null ? prevValue.YValue : 0
                                }
                            );
                        }
                    }
                }

                // もし１日しかなかったら、グラフが描写されないので、二日分にする。
                if(allDates.Count == 1)
                {
                    foreach(var page in allSeries)
                    {
                        page.Items.Add(new DataItem() { XValue = page.Items[0].XValue.AddDays(-1), YValue = page.Items[0].YValue });
                    }
                }

                return allSeries;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Task getProjectsTask = null;
            Models.Managers.RedmineManager.Default.Where(r => r != null).SubscribeWithErr(r =>
            {
                getProjectsTask = Task.Run(() =>
                {
                    var ps = r.GetMyProjectsOnlyWikiEnabled();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Projects.Clear();
                        foreach (var p in ps)
                        {
                            Projects.Add(p);
                        }
                    });
                });
            });

            IsSelected.SubscribeWithErr(async _ =>
            {
                if (getProjectsTask != null && !getProjectsTask.IsCompleted)
                {
                    await getProjectsTask;
                }
            }).AddTo(disposables);

            SelectedProject.Where(p => p != null).SubscribeWithErr(p => ReloadCommand.Command.Execute(null)).AddTo(disposables);

            ReloadCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdReload, Properties.Resources.reload,
                SelectedProject.Select(p => p != null ? null : ""),
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(ReloadCommand));
                    using (IsBusy.ProcessStart())
                    {
                        WikiPages.Clear();

                        var wikiItems = await Task.Run(() => Models.Managers.RedmineManager.Default.Value.GetAllWikiPages(SelectedProject.Value.Identifier));
                        foreach (var wikiItem in wikiItems.Where(a => string.IsNullOrEmpty(a.ParentTitle)))
                        {
                            var wiki = new MyWikiPageCount(wikiItem, null, wikiItems);
                            var _ = wiki.UpdateSummaryAsync(Models.Managers.RedmineManager.Default.Value); // 投げ捨て
                            WikiPages.Add(wiki);
                        }
                    }
                });

            var isBusyUpdateSummary = WikiPages.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
            {
                if (WikiPages.Any())
                    return WikiPages.Select(a => a.IsBusyUpdateSummary).CombineLatestValuesAreAllFalse().Inverse().ToReadOnlyReactivePropertySlim();
                else
                    return new BusyNotifier().ToReadOnlyReactivePropertySlim(false);
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            ExportCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdCSV, Properties.Resources.export_excel,
                isBusyUpdateSummary.ObserveProperty(a => a.Value.Value).Select(a => a ? "" : null),
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(ExportCommand));
                    var dialog = new SaveFileDialog();
                    dialog.FileName = $"{SelectedProject.Value.Name}_{DateTime.Today.ToString("yyMMdd")}.csv";
                    dialog.Filter = "CSV Files|*.csv";
                    if (dialog.ShowDialog().Value == true)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine(addDoubleQuat(Properties.Resources.WikiPageResultProject, SelectedProject.Value.Name));
                        sb.AppendLine();
                        sb.AppendLine(addDoubleQuat(Properties.Resources.WikiPageResultTitle,
                                                    Properties.Resources.WikiPageResultParentPage,
                                                    Properties.Resources.WikiPageResultUpdateOn,
                                                    Properties.Resources.WikiPageResultAuthor,
                                                    Properties.Resources.WikiPageResultNoOfChar,
                                                    Properties.Resources.WikiPageResultNoOfLine,
                                                    Properties.Resources.WikiPageResultNoOfCharIncluded,
                                                    Properties.Resources.WikiPageResultNoOfLineIncluded));
                        var wikis = WikiPages.Flatten(w => w.Children).ToList();
                        foreach (var w in wikis)
                        {
                            sb.AppendLine(addDoubleQuat(w.IndexedTitle, w.ParentTitle, w.UpdatedOn, w.Author.Name, w.Summary.NoOfChar, w.Summary.NoOfLine,
                                                        w.SummaryIncludedChildren.NoOfChar, w.SummaryIncludedChildren.NoOfLine));
                        }
                        FileHelper.WriteAllText(dialog.FileName, sb.ToString());
                    }
                }).AddTo(disposables);
        }


        private string addDoubleQuat(params object[] strs)
        {
            return "\"" + string.Join("\",\"", strs) + "\"";
        }
    }
}
