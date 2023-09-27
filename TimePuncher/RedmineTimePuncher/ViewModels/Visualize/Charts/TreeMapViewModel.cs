using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.FactorTypes;
using RedmineTimePuncher.ViewModels.Bases;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using RedmineTimePuncher.ViewModels.Visualize.TreeMapItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.Visualize.Charts
{
    public class TreeMapViewModel : ChartViewModelBase
    {
        public FactorTypeViewModel FirstGroupingType { get; set; }
        public FactorTypeViewModel SecondGroupingType { get; set; }
        public FactorTypeViewModel ThirdGroupingType { get; set; }

        public ReactivePropertySlim<ObservableCollection<TreeMapItemViewModelBase>> Points { get; set; }
        public ObservableCollection<TreeMapItemViewModelBase> SelectedPoints { get; set; }

        public ReactiveCommand<int> GoToTicketCommand { get; set; }
        public ReactiveCommand ExpandCommand { get; set; }
        public ReactiveCommand CollapseCommand { get; set; }
        public ReactiveCommand RemoveCommand { get; set; }

        public TreeMapViewModel(ResultViewModel parent) : base(ViewType.TreeMap, parent)
        {
            var groupings = new[] { FactorType.None, FactorType.Project, FactorType.Category, FactorType.User, FactorType.OnTime };
            FirstGroupingType = new FactorTypeViewModel("グルーピング１", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.FirstGrouping), groupings).AddTo(disposables);
            FirstGroupingType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            SecondGroupingType = new FactorTypeViewModel("グルーピング２", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.SecondGrouping), groupings).AddTo(disposables);
            SecondGroupingType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            ThirdGroupingType = new FactorTypeViewModel("グルーピング３", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.ThirdGrouping), groupings).AddTo(disposables);
            ThirdGroupingType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            Points = new ReactivePropertySlim<ObservableCollection<TreeMapItemViewModelBase>>().AddTo(disposables);
            SelectedPoints = new ObservableCollection<TreeMapItemViewModelBase>();
            SelectedPoints.CollectionChangedAsObservable().Subscribe(_ =>
            {
                updateTreeListSelection(SelectedPoints.Where(p => p.Issue != null).Select(p => p.Issue.Id).ToArray());
            }).AddTo(disposables);

            this.factors = new List<FactorTypeViewModel>() { FirstGroupingType, SecondGroupingType, ThirdGroupingType };
            setupIsEdited();

            GoToTicketCommand = new ReactiveCommand<int>().WithSubscribe(id =>
            {
                System.Diagnostics.Process.Start(MyIssue.GetUrl(id));
            }).AddTo(disposables);

            ExpandCommand = new ReactiveCommand().WithSubscribe(() => exec(i => parent.ExpandAll(i))).AddTo(disposables);
            CollapseCommand = new ReactiveCommand().WithSubscribe(() => exec(i => parent.CollapseAll(i))).AddTo(disposables);
            RemoveCommand = new ReactiveCommand().WithSubscribe(() => exec(i => parent.RemoveTicket(i))).AddTo(disposables);
        }

        private void exec(Action<int> action)
        {
            var childIds = SelectedPoints.OfType<GroupingItemViewModel>()
                                    .SelectMany(g => getChildTickets(g))
                                    .Select(t => t.Issue.Id).Distinct().ToList();
            var ids = SelectedPoints.OfType<TicketItemViewModel>().Select(i => i.Issue.Id).Distinct().ToList();
            foreach (var i in childIds.Concat(ids).Distinct())
            {
                action.Invoke(i);
            }
        }

        private List<TicketItemViewModel> getChildTickets(GroupingItemViewModel top)
        {
            if (top.Children[0] is TicketItemViewModel)
            {
                return top.Children.OfType<TicketItemViewModel>().ToList();
            }
            else
            {
                var children = new List<TicketItemViewModel>();
                foreach (var group in top.Children)
                {
                    children.AddRange(getChildTickets(group as GroupingItemViewModel));
                }
                return children;
            }
        }

        public override void SetupSeries(bool needsSetVisible = true)
        {
            var points = new ObservableCollection<TreeMapItemViewModelBase>();

            var allTimeEntries = getAllTimeEntries();

            if (FirstGroupingType.SelectedType.Value == FactorType.None)
            {
                createTicketTree(allTimeEntries).ForEach(t => points.Add(t));
            }
            else if (SecondGroupingType.SelectedType.Value == FactorType.None)
            {
                foreach (var first in allTimeEntries.GroupBy(p => p.GetFactor(FirstGroupingType.SelectedType.Value)))
                {
                    var firstGroup = new GroupingItemViewModel(first.Key, 1, this);
                    createTicketTree(first.ToList()).ForEach(t => firstGroup.Children.Add(t));
                    points.Add(firstGroup);
                }
            }
            else if (ThirdGroupingType.SelectedType.Value == FactorType.None)
            {
                foreach (var first in allTimeEntries.GroupBy(p => p.GetFactor(FirstGroupingType.SelectedType.Value)))
                {
                    var firstGroup = new GroupingItemViewModel(first.Key, 1, this);
                    foreach (var second in first.GroupBy(p => p.GetFactor(SecondGroupingType.SelectedType.Value)))
                    {
                        var secondGroup = new GroupingItemViewModel(second.Key, 2, this);
                        createTicketTree(second.ToList()).ForEach(t => secondGroup.Children.Add(t));
                        firstGroup.Children.Add(secondGroup);
                    }
                    points.Add(firstGroup);
                }
            }
            else
            {
                foreach (var first in allTimeEntries.GroupBy(p => p.GetFactor(FirstGroupingType.SelectedType.Value)))
                {
                    var firstGroup = new GroupingItemViewModel(first.Key, 1, this);
                    foreach (var second in first.GroupBy(p => p.GetFactor(SecondGroupingType.SelectedType.Value)))
                    {
                        var secondGroup = new GroupingItemViewModel(second.Key, 2, this);
                        foreach (var third in first.GroupBy(p => p.GetFactor(ThirdGroupingType.SelectedType.Value)))
                        {
                            var thirdGroup = new GroupingItemViewModel(third.Key, 3, this);
                            createTicketTree(third.ToList()).ForEach(t => thirdGroup.Children.Add(t));
                            secondGroup.Children.Add(thirdGroup);
                        }
                        firstGroup.Children.Add(secondGroup);
                    }
                    points.Add(firstGroup);
                }
            }

            foreach (var p in points)
            {
                p.SetTotalHours();
            }

            var preSelected = SelectedPoints.Where(p => p.Issue != null).Select(t => t.Issue.Id).ToArray();

            Points.Value = points;

            SelectTickets(preSelected);
        }

        private List<TreeMapItemViewModelBase> createTicketTree(List<PersonHourModel> allTimeEntries)
        {
            var visibleTickets = parent.Tickets.SelectMany(t => t.GetVisibleTickets()).ToList();

            var allPoints = visibleTickets.Select(t =>
            {
                var ps = allTimeEntries.Where(p => p.RawIssue.Id == t.Model.RawIssue.Id).ToArray();
                if (ps.Length == 0)
                    return new TicketItemViewModel(t.Model.RawIssue, this).AddTo(disposables);

                var model = new PersonHourModel(t.Model.RawIssue, ps);
                return new TicketItemViewModel(model, this).AddTo(disposables);
            }).Where(a => a != null).ToList();

            foreach (var t in visibleTickets)
            {
                var parent = allPoints.FirstOrDefault(p => p.Issue.Id == t.Model.RawIssue.Id);
                if (parent == null)
                    continue;

                foreach (var c in t.Children)
                {
                    var child = allPoints.FirstOrDefault(p => p.Issue.Id == c.Model.RawIssue.Id);
                    if (child != null)
                    {
                        parent.Children.Add(child);
                    }
                }

                if (parent.Children.Any() && parent.Hours > 0)
                {
                    parent.Children.Insert(0, new TicketItemViewModel(parent, this));
                }
            }

            var points = new List<TreeMapItemViewModelBase>();
            foreach (var t in parent.Tickets)
            {
                var top = allPoints.FirstOrDefault(p => p.Issue.Id == t.Model.RawIssue.Id);
                if (top != null)
                {
                    points.Add(top);
                }
            }

            return points;
        }

        private BusyNotifier nowTicketUpdating = new BusyNotifier();
        private void updateTreeListSelection(params int[] ids)
        {
            if (nowTicketUpdating.IsBusy)
                return;

            parent.SelectTickets(ids);
        }

        public void SelectTickets(params int[] ids)
        {
            using (nowTicketUpdating.ProcessStart())
            {
                // TreeMap の SelectedItems は IList で Add や Remove に対応していないため、
                // SelectedPoints に Add しても NotSupportedException になる。
                // よって、このような対応とする。
                foreach (var p in SelectedPoints.ToList())
                {
                    p.IsSelected = false;
                }
                foreach (var t in Points.Value.Flatten(p => p.Children).Where(p => p.Issue != null && ids.Contains(p.Issue.Id)).ToList())
                {
                    t.IsSelected = true;
                }
            }
        }
    }
}
