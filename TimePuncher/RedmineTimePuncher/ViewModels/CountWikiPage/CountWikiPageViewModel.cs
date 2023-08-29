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
                    await Task.Run(() =>
                    {
                        if (p != null)
                        {
                            var wikis = parent.Redmine.Value.GetAllWikiPages(p.Identifier);
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                AllWikiPages.Clear();
                                foreach (var w in wikis.OrderByDescending(w => w.IsTopWiki))
                                {
                                    AllWikiPages.Add(w);
                                }
                                SelectedWikiPage.Value = AllWikiPages.FirstOrDefault();
                            });
                        }
                    });
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
                        await Task.Run(() =>
                        {
                            var topWiki = parent.Redmine.Value.GetWikiPageIncludeChildren(SelectedWikiPage.Value.ProjectId, SelectedWikiPage.Value.Title);
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Result.Value = new CountResultViewModel(DateTime.Now, parent.Redmine.Value.MyUser.Name, SelectedProject.Value, topWiki,
                                                                        IsRegex ? FilterType.AsRegex : FilterType.Contains, DisableWords).AddTo(disposables);
                                WikiPages.Clear();
                                if (Result.Value.TopWikiPage.IsSummaryTarget)
                                    WikiPages.Add(Result.Value.TopWikiPage);
                            });
                        });
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
