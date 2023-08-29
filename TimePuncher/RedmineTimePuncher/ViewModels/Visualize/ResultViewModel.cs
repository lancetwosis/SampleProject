using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.ViewModels.Visualize.Charts;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using RedmineTimePuncher.ViewModels.Visualize.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class ResultViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public TicketFiltersViewModel Filters { get; set; }
        public ObservableCollection<TicketViewModel> Tickets { get; set; }
        public ObservableCollection<TicketViewModel> AllTickets { get; set; }

        public ReactivePropertySlim<ViewType> ViewType { get; set; }
        public BarChartViewModel BarChart { get; set; }
        public PieChartViewModel PieChart { get; set; }
        public TreeMapViewModel TreeMap { get; set; }

        public ResultModel Model { get; set; }

        private VisualizeViewModel parent { get; set; }

        public ResultViewModel(VisualizeViewModel parent)
        {
            this.parent = parent;

            Tickets = new ObservableCollection<TicketViewModel>();
            AllTickets = new ObservableCollection<TicketViewModel>();

            var json = Properties.Settings.Default.VisualizeResult;
            if (!string.IsNullOrEmpty(json))
                Model = CloneExtentions.ToObject<ResultModel>(json);
            else
                Model = new ResultModel();

            ViewType = Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.ViewType).AddTo(disposables);
            ViewType.Skip(1).Subscribe(_ => updateSerieses());

            BarChart = new BarChartViewModel(this).AddTo(disposables);
            PieChart = new PieChartViewModel(this).AddTo(disposables);
            TreeMap = new TreeMapViewModel(this).AddTo(disposables);
        }

        public void LoadPreviousResult()
        {
            if (Model.HasValue)
            {
                var filters = new TicketFiltersViewModel(parent, Model.Filters, Model.CreateAt).AddTo(disposables);
                setupTimeEntries();

                Filters = filters;
                Model.Filters = Filters.Model;
            }
        }

        private CompositeDisposable myDisposables;
        private void setupTimeEntries()
        {
            if (Model.TimeEntries.Count == 0)
            {
                clear();
                return;
            }

            try
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                var timesDic = Model.TimeEntries.Select(a => new MyTimeEntry(a)).GroupBy(t => (t.Entry.Issue, t.Entry.User, t.SpentOn, t.Entry.Activity, t.Type), t => t);
                var rawEntries = timesDic.Select(p =>
                {
                    var issue = Model.Tickets.First(i => i.RawIssue.Id == p.Key.Issue.Id).RawIssue;
                    var project = Model.Projects.First(a => a.Id == issue.Project.Id);
                    var category = Model.Categories.First(a => a.Name == p.Key.Activity.Name);
                    return new PersonHourModel(issue, project, p.Key.User, p.Key.SpentOn.Value, category, p.Key.Type, p.ToList()).AddTo(myDisposables);
                }).ToList();

                var tickets = Model.Tickets.Select(a => new TicketViewModel(a).AddTo(myDisposables)).ToList();
                // TimeEntry をそれぞれのチケットに紐づける
                foreach (var e in rawEntries)
                {
                    var t = tickets.First(a => a.Model.RawIssue.Id == e.RawIssue.Id);
                    t.TimeEntries.Add(e);
                }

                // それぞれのチケットの親子関係を設定
                foreach (var t in tickets)
                {
                    foreach (var child in tickets.Where(a => a.Model.RawIssue.ParentIssue?.Id == t.Model.RawIssue.Id))
                    {
                        t.Children.Add(child);
                        child.Parent = t;
                    }
                }

                AllTickets.Clear();
                tickets.Where(t => t.Children.Any() || t.HasTimeEntry.Value).ToList().ForEach(a => AllTickets.Add(a));
                Tickets.Clear();
                foreach (var t in AllTickets.Where(a => a.Parent == null))
                {
                    Tickets.Add(t);
                }

                AllTickets.Select(a => a.IsEnabled).CombineLatest().Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnUIDispatcher().Subscribe(_ =>
                {
                    updateSerieses();
                }).AddTo(myDisposables);

                AllTickets.Select(a => a.IsExpanded).CombineLatest().Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnUIDispatcher().Subscribe(_ =>
                {
                    updateSerieses();
                }).AddTo(myDisposables);
            }
            catch (Exception e)
            {
                Model.HasValue = false;
                clear();

                throw new ApplicationException("チャートの作成に失敗しました。", e);
            }
        }

        private void clear()
        {
            AllTickets.Clear();
            Tickets.Clear();

            updateSerieses();
        }

        public async Task GetTimeEntriesAsync(TicketFiltersViewModel filters)
        {
            Model.Projects = await Task.Run(() => parent.Parent.Redmine.Value.Projects.Value);
            Model.TimeEntries = filters.GetTimeEntries();
            Model.Tickets = await filters.GetTicketsAsync(Model.TimeEntries);
            Model.Categories = parent.Parent.Settings.Category.Items.ToList();

            setupTimeEntries();

            // 親チケット指定が設定されていたらトップの子チケットだけが展開された状態にする
            if (Tickets.Any() && filters.SpecifyParentIssue.IsEnabled.Value)
            {
                foreach (var c in Tickets[0].Children)
                {
                    c.Collapse();
                }
            }

            Model.HasValue = true;
            Model.CreateAt = DateTime.Now;

            Filters = filters.Clone(Model.CreateAt).AddTo(myDisposables);
            Model.Filters = Filters.Model;
        }

        public async Task UpdateTimeEntriesAsync()
        {
            await GetTimeEntriesAsync(Filters);
        }

        private void updateSerieses()
        {
            switch (ViewType.Value)
            {
                case Enums.ViewType.BarChart:
                    BarChart.SetupSeries();
                    break;
                case Enums.ViewType.PieChart:
                    PieChart.SetupSeries();
                    break;
                case Enums.ViewType.TreeMap:
                    TreeMap.SetupSeries();
                    break;
            }
        }

        public void Save()
        {
            Properties.Settings.Default.VisualizeResult = Model.ToJson();
            Properties.Settings.Default.Save();
        }
    }
}
