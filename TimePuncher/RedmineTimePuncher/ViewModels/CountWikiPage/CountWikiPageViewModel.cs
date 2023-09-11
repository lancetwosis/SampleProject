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
using RedmineTimePuncher.ViewModels.CountWikiPage.Charts;
using System.Diagnostics;

namespace RedmineTimePuncher.ViewModels.CountWikiPage
{
    public class CountWikiPageViewModel : FunctionViewModelBase
    {
        public BusyNotifier IsBusy { get; set; }
        public ObservableCollection<MyProject> Projects { get; set; }
        public ReactivePropertySlim<MyProject> SelectedProject { get; set; }

        public ObservableCollection<MyWikiPage> AllWikiPages { get; set; }
        public ReactivePropertySlim<MyWikiPage> SelectedWikiPage { get; set; }

        public string DisableWords { get; set; }
        public bool IsRegex { get; set; }

        public AsyncCommandBase CountCommand { get; set; }
        public AsyncCommandBase ExportCommand { get; set; }

        public ReactivePropertySlim<CountResultViewModel> Result { get; set; }
        public ReadOnlyReactivePropertySlim<string> CountedAt { get; set; }
        public ReadOnlyReactivePropertySlim<string> TargetProject { get; set; }

        public ObservableCollection<MyWikiPage> WikiPages { get; set; }
        public ObservableCollection<MyWikiPage> SelectedWikiPages { get; set; }
        public ReadOnlyReactivePropertySlim<ReadOnlyReactivePropertySlim<bool>> IsBusyUpdateHistories { get; set; }
        public ReadOnlyReactivePropertySlim<List<HistorySummary>> HistorySummaries { get; set; }
        public ObservableCollection<HistorySummary> SelectedHistorySummaries { get; set; }
        public AsyncCommandBase DiffHistoriesCommand { get; set; }
        public ReadOnlyReactivePropertySlim<List<UserSummary>> UserSummaries { get; set; }
        public ReadOnlyReactivePropertySlim<List<SeriesViewModelBase>> Serieses { get; set;}


        public CountWikiPageViewModel(MainWindowViewModel parent) : base(ApplicationMode.WikiPageCounter, parent)
        {
            IsBusy = new BusyNotifier();

            Projects = new ObservableCollection<MyProject>();
            SelectedProject = new ReactivePropertySlim<MyProject>().AddTo(disposables);
            AllWikiPages = new ObservableCollection<MyWikiPage>();
            SelectedWikiPage = new ReactivePropertySlim<MyWikiPage>().AddTo(disposables);

            Result = new ReactivePropertySlim<CountResultViewModel>().AddTo(disposables);
            CountedAt = Result.Select(r => r != null ? r.CountedAt.ToString() : "").ToReadOnlyReactivePropertySlim().AddTo(disposables);
            TargetProject = Result.Select(r => r != null ? r.Project.Name : "").ToReadOnlyReactivePropertySlim().AddTo(disposables);

            WikiPages = new ObservableCollection<MyWikiPage>();
            SelectedWikiPages = new ObservableCollection<MyWikiPage>();

            SelectedWikiPages.ObserveAddChanged().Subscribe(async wiki =>
                await wiki.UpdateHistoriesAsync(parent.Redmine.Value));

            IsBusyUpdateHistories = 
                SelectedWikiPages.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                {
                    if (SelectedWikiPages.Any())
                        return SelectedWikiPages.Select(a => a.IsBusyUpdateHistories).CombineLatestValuesAreAllFalse().Inverse().ToReadOnlyReactivePropertySlim();
                    else
                        return new BusyNotifier().ToReadOnlyReactivePropertySlim(false);
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectedHistorySummaries = new ObservableCollection<HistorySummary>();

            DiffHistoriesCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdMeasure, Properties.Resources.icons8_count,
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

            HistorySummaries = IsBusyUpdateHistories.ObserveProperty(a => a.Value.Value).Select(a =>
            {
                if (!a)
                    return SelectedWikiPages.SelectMany(h => h.Histories).OrderByDescending(b => b.UpdatedOn).ToList();
                else
                    return new List<HistorySummary>();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            UserSummaries = HistorySummaries.Select(allhs => 
            {
                return allhs.GroupBy(h => h.Author).Select(x => new UserSummary()
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

                return allSeries;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Task getProjectsTask = null;
            parent.Redmine.Where(r => r != null).SubscribeWithErr(r =>
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
                        SelectedProject.Value = Projects.FirstOrDefault();
                    });
                });
            });

            parent.Mode.Where(m => m == Enums.ApplicationMode.WikiPageCounter).SubscribeWithErr(async _ =>
            {
                if (getProjectsTask != null && !getProjectsTask.IsCompleted)
                    await getProjectsTask;
            }).AddTo(disposables);

            SelectedProject.SubscribeWithErr(async p =>
            {
                using (IsBusy.ProcessStart())
                using (parent.IsBusy.ProcessStart(""))
                {
                    if (p != null)
                    {
                        var wikis = await Task.Run(() =>
                            parent.Redmine.Value.GetAllWikiPages(p.Identifier));
                        AllWikiPages.Clear();
                        foreach (var w in wikis.OrderByDescending(w => w.IsTopWiki))
                        {
                            AllWikiPages.Add(w);
                        }
                        SelectedWikiPage.Value = AllWikiPages.FirstOrDefault();
                    }
                }
            }).AddTo(disposables);

            CountCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdMeasure, Properties.Resources.icons8_count,
                SelectedWikiPage.Select(w => w != null ? null : ""),
                async () =>
                {
                    using (IsBusy.ProcessStart())
                    using (parent.IsBusy.ProcessStart(""))
                    {
                        var topWiki = await Task.Run(() =>
                             parent.Redmine.Value.GetWikiPageIncludeChildren(SelectedWikiPage.Value.ProjectId, SelectedWikiPage.Value.Title));
                        Result.Value = new CountResultViewModel(DateTime.Now, parent.Redmine.Value.MyUser.Name, SelectedProject.Value, topWiki,
                                                                IsRegex ? FilterType.AsRegex : FilterType.Contains, DisableWords).AddTo(disposables);
                        WikiPages.Clear();
                        if (Result.Value.TopWikiPage.IsSummaryTarget)
                            WikiPages.Add(Result.Value.TopWikiPage);
                    }
                }).AddTo(disposables);

            ExportCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdCSV, Properties.Resources.export_excel,
                Result.Select(r => r != null && r.TopWikiPage.IsSummaryTarget ? null : ""),
                async () =>
                {
                    var dialog = new SaveFileDialog();
                    dialog.FileName = Result.Value.FileName;
                    dialog.Filter = "CSV Files|*.csv";
                    if (dialog.ShowDialog().Value == true)
                    {
                        Result.Value.Export(dialog.FileName);
                    }
                }).AddTo(disposables);
        }
    }
}
