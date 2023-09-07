﻿using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
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

        private ResultViewModel parent { get; set; }

        public TreeMapViewModel(ResultViewModel parent) : base(ViewType.TreeMap, parent)
        {
            this.parent = parent;

            var groupings = new[] { FactorType.None, FactorType.Project, FactorType.Category, FactorType.User, FactorType.OnTime };
            FirstGroupingType = new FactorTypeViewModel("グルーピング１", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.FirstGrouping), groupings).AddTo(disposables);
            FirstGroupingType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            SecondGroupingType = new FactorTypeViewModel("グルーピング２", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.SecondGrouping), groupings).AddTo(disposables);
            SecondGroupingType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            ThirdGroupingType = new FactorTypeViewModel("グルーピング３", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.ThirdGrouping), groupings).AddTo(disposables);
            ThirdGroupingType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            Points = new ReactivePropertySlim<ObservableCollection<TreeMapItemViewModelBase>>().AddTo(disposables);
            this.factors = new List<FactorTypeViewModel>() { FirstGroupingType, SecondGroupingType, ThirdGroupingType };
            setupIsEdited();
        }

        public override void SetupSeries()
        {
            var points = new ObservableCollection<TreeMapItemViewModelBase>();

            var allTimeEntries = parent.Tickets.SelectMany(t => t.GetAllTimeEntries()).ToList();

            if (FirstGroupingType.SelectedType.Value == FactorType.None)
            {
                createTicketTree(allTimeEntries).ForEach(t => points.Add(t));
            }
            else if (SecondGroupingType.SelectedType.Value == FactorType.None)
            {
                foreach (var first in allTimeEntries.GroupBy(p => p.GetFactor(FirstGroupingType.SelectedType.Value)))
                {
                    var firstGroup = new GroupingItemViewModel(first.Key, 1);
                    createTicketTree(first.ToList()).ForEach(t => firstGroup.Children.Add(t));
                    points.Add(firstGroup);
                }
            }
            else if (ThirdGroupingType.SelectedType.Value == FactorType.None)
            {
                foreach (var first in allTimeEntries.GroupBy(p => p.GetFactor(FirstGroupingType.SelectedType.Value)))
                {
                    var firstGroup = new GroupingItemViewModel(first.Key, 1);
                    foreach (var second in first.GroupBy(p => p.GetFactor(SecondGroupingType.SelectedType.Value)))
                    {
                        var secondGroup = new GroupingItemViewModel(second.Key, 2);
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
                    var firstGroup = new GroupingItemViewModel(first.Key, 1);
                    foreach (var second in first.GroupBy(p => p.GetFactor(SecondGroupingType.SelectedType.Value)))
                    {
                        var secondGroup = new GroupingItemViewModel(second.Key, 2);
                        foreach (var third in first.GroupBy(p => p.GetFactor(ThirdGroupingType.SelectedType.Value)))
                        {
                            var thirdGroup = new GroupingItemViewModel(third.Key, 3);
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

            Points.Value = points;
        }

        private List<TreeMapItemViewModelBase> createTicketTree(List<PersonHourModel> allTimeEntries)
        {
            var visibleTickets = parent.Tickets.SelectMany(t => t.GetVisibleTickets()).ToList();

            var allPoints = visibleTickets.Select(t =>
            {
                var ps = allTimeEntries.Where(p => p.RawIssue.Id == t.Model.RawIssue.Id).ToArray();
                if (ps.Length == 0)
                    return new TicketItemViewModel(t.Model.RawIssue).AddTo(disposables);

                var model = new PersonHourModel(t.Model.RawIssue, ps);
                return new TicketItemViewModel(model).AddTo(disposables);
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
                    parent.Children.Insert(0, new TicketItemViewModel(parent));
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
    }
}