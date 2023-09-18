using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
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

        public ReadOnlyReactivePropertySlim<bool> IsEdited { get; set; }

        public ObservableCollection<TicketViewModel> Tickets { get; set; }
        public ObservableCollection<TicketViewModel> AllTickets { get; set; }
        public ObservableCollection<TicketViewModel> SelectedTickets { get; set; }
        public ResultFiltersViewModel ResultFilters { get; set; }

        public ReactivePropertySlim<ViewType> ViewType { get; set; }
        public BarChartViewModel BarChart { get; set; }
        public PieChartViewModel PieChart { get; set; }
        public TreeMapViewModel TreeMap { get; set; }

        public ResultModel Model { get; set; }

        private VisualizeViewModel parent { get; set; }

        public ResultViewModel(VisualizeViewModel parent)
        {
            this.parent = parent;

            var json = Properties.Settings.Default.VisualizeResult;
            try
            {
                if (!string.IsNullOrEmpty(json))
                    Model = CloneExtentions.ToObject<ResultModel>(json);
                else
                    Model = new ResultModel();
            }
            catch
            {
                Model = new ResultModel();
            }

            IsEdited = this.ObserveProperty(a => a.Model.IsEdited).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Tickets = new ObservableCollection<TicketViewModel>();
            AllTickets = new ObservableCollection<TicketViewModel>();
            SelectedTickets = new ObservableCollection<TicketViewModel>();
            SelectedTickets.CollectionChangedAsObservable().StartWithDefault().Subscribe(_ => 
            {
                updateViewSelection(SelectedTickets.Select(t => t.Model.RawIssue.Id).ToArray());
            }).AddTo(disposables);
        }

        private CompositeDisposable initializeDisposables;
        public void Initialize()
        {
            initializeDisposables?.Dispose();
            initializeDisposables = new CompositeDisposable().AddTo(disposables);

            ViewType = Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.ViewType).AddTo(initializeDisposables);
            ViewType.Skip(1).Subscribe(_ => updateSerieses(true));

            BarChart = new BarChartViewModel(this).AddTo(initializeDisposables);
            PieChart = new PieChartViewModel(this).AddTo(initializeDisposables);
            TreeMap = new TreeMapViewModel(this).AddTo(initializeDisposables);

            new[] { BarChart.IsEdited, PieChart.IsEdited, TreeMap.IsEdited }.CombineLatest()
                .Subscribe(l =>
                {
                    if (l.Any(a => a))
                        Model.IsEdited = true;
                }).AddTo(initializeDisposables);

            if (Model.HasValue)
            {
                var filters = new TicketFiltersViewModel(parent, Model.Filters, Model.CreateAt).AddTo(initializeDisposables);
                setupTicketTree();

                Filters = filters;
                Model.Filters = Filters.Model;
            }
        }

        private CompositeDisposable setupTreeDisposables;
        private void setupTicketTree(bool isNew = false)
        {
            if (Model.TimeEntries.Count == 0)
            {
                clear();
                return;
            }

            try
            {
                setupTreeDisposables?.Dispose();
                setupTreeDisposables = new CompositeDisposable().AddTo(disposables);

                FactorType.CustomFields.Clear();
                Model.CustomFields.ForEach(f => FactorType.CustomFields.Add(new FactorType(f)));

                var timesDic = Model.TimeEntries.Select(a => new MyTimeEntry(a)).GroupBy(t => (t.Entry.Issue, t.Entry.User, t.SpentOn, t.Entry.Activity, t.Type), t => t);
                var rawEntries = timesDic.Select(p =>
                {
                    var issue = Model.Tickets.First(i => i.RawIssue.Id == p.Key.Issue.Id).RawIssue;
                    var project = Model.Projects.First(a => a.Id == issue.Project.Id);
                    var category = Model.Categories.First(a => a.Name == p.Key.Activity.Name);
                    return new PersonHourModel(issue, project, p.Key.User, p.Key.SpentOn.Value, category, p.Key.Type, p.ToList()).AddTo(setupTreeDisposables);
                }).ToList();

                var cs = rawEntries.SelectMany(a => a.CustomFields).Select(a => a.Type).Distinct().ToList();
                foreach (var c in cs)
                {
                    BarChart.XAxisType.Types.Add(c);
                }

                var tickets = Model.Tickets.Select(a => new TicketViewModel(a).AddTo(setupTreeDisposables)).ToList();
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

                ResultFilters = new ResultFiltersViewModel(this, rawEntries, isNew).AddTo(disposables);

                var onIsEnable = AllTickets.Select(a => a.IsEnabled).CombineLatest().Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnUIDispatcher();
                var onIsExpanded = AllTickets.Select(a => a.ObserveProperty(b => b.IsExpanded)).CombineLatest().Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnUIDispatcher();
                onIsEnable.CombineLatest(onIsExpanded, (_1, _2) => true).Skip(1).Subscribe(_ => updateSerieses(true)).AddTo(setupTreeDisposables);

                var onFiltersChanged = ResultFilters.Items.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                    ResultFilters.Items.ObserveElementProperty(i => i.IsEnabled.Value),
                    ResultFilters.Items.ObserveElementProperty(i => i.NowEditing.Value), (_1, _2, _3) => true);
                onFiltersChanged.Skip(1).Subscribe(_ => updateSerieses(true, false));

                updateSerieses();
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

        public async Task GetTimeEntriesAsync(TicketFiltersViewModel filters, bool isNew = true)
        {
            var r = saveIfNeeded();
            if (!r.HasValue)
                return;

            Model.Projects = await Task.Run(() => parent.Parent.Redmine.Value.Projects.Value);
            Model.CustomFields = await Task.Run(() => parent.Parent.Redmine.Value.CustomFields.Value);
            Model.TimeEntries = filters.GetTimeEntries();
            Model.Tickets = await filters.GetTicketsAsync(Model.TimeEntries);
            Model.Categories = parent.Parent.Settings.Category.Items.ToList();

            setupTicketTree(isNew);

            // 親チケット指定が設定されていたらトップの子チケットだけが展開された状態にする
            if (Tickets.Any() && filters.SpecifyParentIssue.IsEnabled.Value)
            {
                foreach (var c in Tickets[0].Children)
                {
                    c.SetIsExpanded(false, true);
                }
            }

            Model.HasValue = true;
            Model.CreateAt = DateTime.Now;
            Model.IsEdited = true;
            // 再取得の場合はファイル名をクリアしない
            if (isNew)
                Model.FileName = null;

            Filters = filters.Clone(Model.CreateAt).AddTo(setupTreeDisposables);
            Model.Filters = Filters.Model;
        }

        public async Task UpdateTimeEntriesAsync()
        {
            await GetTimeEntriesAsync(Filters, false);
        }

        public void Open()
        {
            var r = saveIfNeeded();
            if (!r.HasValue)
                return;

            var dialog = new OpenFileDialog();
            dialog.Filter = "Time Entries Result file(*.rtr)|*.rtr|All file(*.*)|*.*";
            if (!dialog.ShowDialog().Value)
                return;

            ResultModel desirialized;
            try
            {
                desirialized = CloneExtentions.ToObject<ResultModel>(System.IO.File.ReadAllText(dialog.FileName));
            }
            catch
            {
                throw new ApplicationException($"結果ファイルを開けませんでした。{Environment.NewLine}{dialog.FileName}");
            }

            Model = desirialized;
            Model.FileName = dialog.FileName;

            Initialize();
        }

        private bool? saveIfNeeded()
        {
            if (!Model.HasValue || (!string.IsNullOrEmpty(Model.FileName) && !IsEdited.Value))
                return false;

            var needsSave = MessageBoxHelper.ConfirmQuestion("現在の結果が破棄されます。現在の結果を保存しますか？", MessageBoxHelper.ButtonType.YesNoCancel);
            if (!needsSave.HasValue)
                return null;

            if (!needsSave.Value)
                return false;

            try
            {
                SaveToFile();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SaveToFile()
        {
            if (string.IsNullOrEmpty(Model.FileName))
                SaveAsToFile();
            else
                save(Model.FileName);
        }

        public void SaveAsToFile()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Time Entries Result file(*.rtr)|*.rtr|All file(*.*)|*.*";
            if (dialog.ShowDialog().Value)
            {
                save(dialog.FileName);
            }
        }

        private void save(string fileName)
        {
            Model.FileName = fileName;
            Model.IsEdited = false;
            System.IO.File.WriteAllText(Model.FileName, Model.ToJson());

            BarChart.Save();
            PieChart.Save();
            TreeMap.Save();
        }

        private void updateSerieses(bool isEdited = false, bool needsSetVisible = true)
        {
            if (ResultFilters.Items.Any(i => i.NowEditing.Value || !i.IsValid.Value))
                return;

            if (isEdited)
                Model.IsEdited = true;

            switch (ViewType.Value)
            {
                case Enums.ViewType.BarChart:
                    BarChart.SetupSeries(needsSetVisible);
                    break;
                case Enums.ViewType.PieChart:
                    PieChart.SetupSeries(needsSetVisible);
                    break;
                case Enums.ViewType.TreeMap:
                    TreeMap.SetupSeries();
                    break;
            }
        }

        public void ExpandAll(int? id = null)
        {
            if (id.HasValue)
            {
                var target = AllTickets.FirstOrDefault(t => t.Model.RawIssue.Id == id.Value);
                if (target != null)
                    target.SetIsExpanded(true, true);
            }
            else
            {
                foreach (var t in Tickets)
                {
                    t.SetIsExpanded(true, true);
                }
            }
        }

        public void CollapseAll(int? id = null)
        {
            if (id.HasValue)
            {
                var target = AllTickets.FirstOrDefault(t => t.Model.RawIssue.Id == id.Value);
                if (target != null)
                    target.SetIsExpanded(false, true);
            }
            else
            {
                foreach (var t in Tickets)
                {
                    t.SetIsExpanded(false, true);
                }
            }
        }

        public void RemoveTicket(int id)
        {
            var removed = AllTickets.FirstOrDefault(t => t.Model.RawIssue.Id == id);
            if (removed != null)
            {
                removed.IsEnabled.Value = false;
            }
        }

        private BusyNotifier nowTicketUpdating = new BusyNotifier();
        private void updateViewSelection(params int[] ids)
        {
            if (nowTicketUpdating.IsBusy || ViewType == null)
                return;

            if (ViewType.Value == Enums.ViewType.TreeMap)
            {
                TreeMap.SelectTickets(ids);
            }
        }

        public void SelectTickets(params int[] ids)
        {
            using (nowTicketUpdating.ProcessStart())
            {
                foreach (var t in SelectedTickets.ToList())
                {
                    SelectedTickets.Remove(t);
                }
                foreach (var t in AllTickets.Where(t => ids.Contains(t.Model.RawIssue.Id)).ToList())
                {
                    SelectedTickets.Add(t);
                }
            }
        }

        public void AddNewFilter()
        {
            ResultFilters?.AddNewFilter();
        }

        public void Save()
        {
            Properties.Settings.Default.VisualizeResult = Model.ToJson();
            Properties.Settings.Default.Save();
        }
    }
}
