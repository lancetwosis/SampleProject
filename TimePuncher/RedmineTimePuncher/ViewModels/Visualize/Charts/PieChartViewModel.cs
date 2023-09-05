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
using Telerik.Windows.Controls.Legend;

namespace RedmineTimePuncher.ViewModels.Visualize.Charts
{
    public class PieChartViewModel : ChartViewModelBase
    {
        public FactorTypeViewModel CombineType { get; set; }
        public FactorTypeViewModel SortType { get; set; }
        public SeriesViewModel Series { get; set; }

        public FactorTypeViewModel SecondCombineType { get; set; }
        public ReadOnlyReactivePropertySlim<bool> ShowSecondSeries { get; set; }
        public SeriesViewModel SecondSeries { get; set; }

        public TotalLabelViewModel ShowTotal { get; set; }

        public LegendItemCollection LegendItems { get; set; }

        private ResultViewModel parent { get; set; }

        public PieChartViewModel(ResultViewModel parent) : base(ViewType.PieChart, parent)
        {
            this.parent = parent;

            CombineType = new FactorTypeViewModel("グルーピング１", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieCombine),
                FactorType.Issue, FactorType.Project, FactorType.Date, FactorType.User,FactorType.Category, FactorType.OnTime).AddTo(disposables);
            CombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            SecondCombineType = new FactorTypeViewModel("グルーピング２", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieSecondCombine),
                FactorType.None, FactorType.Issue, FactorType.Project, FactorType.Date, FactorType.User, FactorType.Category, FactorType.OnTime).AddTo(disposables);
            SecondCombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            ShowSecondSeries = SecondCombineType.SelectedType.Select(a => a != FactorType.None).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SortType = new FactorTypeViewModel("ソート", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieSort),
                FactorType.None, FactorType.ASC, FactorType.DESC).AddTo(disposables);
            SortType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            ShowTotal = new TotalLabelViewModel(IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieShowTotal)).AddTo(disposables);
        }

        public override void SetupSeries()
        {
            base.SetupSeries();

            var allPoints = parent.Tickets.SelectMany(t => t.GetAllTimeEntries()).ToList();

            var series = new SeriesViewModel(ViewType.PieChart);
            var points = allPoints.GroupBy(a => a.GetFactor(CombineType.SelectedType.Value))
                .Select(a => new PointViewModel(series, a.Key, a.ToList()))
                .ToList();

            if (SortType.SelectedType.Value == FactorType.None)
            {
                points.OrderBy(p => p.Factor.Value).ToList().ForEach(p => series.Points.Add(p));
            }
            else if (SortType.SelectedType.Value == FactorType.ASC)
            {
                points.OrderBy(p => p.Hours).ToList().ForEach(p => series.Points.Add(p));
            }
            else if (SortType.SelectedType.Value == FactorType.DESC)
            {
                points.OrderByDescending(p => p.Hours).ToList().ForEach(p => series.Points.Add(p));
            }

            LegendItems = new LegendItemCollection();
            points.OrderBy(p => p.Factor.Value).Select(p => p.ToLegendItem()).ToList().ForEach(i => LegendItems.Add(i));

            var total =  series.Points.Select(a => a.IsVisble).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            foreach (var p in series.Points.Indexed())
            {
                p.v.SetDisplayValue(total);
                p.v.Index = p.i;
            }

            Series = series;
            ShowTotal.TotalHours = series.Points.Select(p => p.IsVisble).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            if (SecondCombineType.SelectedType.Value != FactorType.None)
            {
                var secondSeries = new SeriesViewModel(ViewType.PieChart);
                var secondPoints = allPoints.GroupBy(a => (a.GetFactor(SecondCombineType.SelectedType.Value), a.GetFactor(CombineType.SelectedType.Value)))
                    .Select(a => new PointViewModel(secondSeries, a.Key.Item1, a.Key.Item2, a.ToList()))
                    .ToList();

                if (SortType.SelectedType.Value == FactorType.None)
                {
                    secondPoints.OrderBy(p => p.ParentFactor.Value).ThenBy(p => p.Factor.Value).ToList().ForEach(p => secondSeries.Points.Add(p));
                }
                else if (SortType.SelectedType.Value == FactorType.ASC)
                {
                    secondPoints.GroupBy(p => p.ParentFactor.Value).OrderBy(g => g.Sum(a => a.Hours))
                        .SelectMany(g => g.OrderBy(a => a.Hours)).ToList().ForEach(p => secondSeries.Points.Add(p));
                }
                else if (SortType.SelectedType.Value == FactorType.DESC)
                {
                    secondPoints.GroupBy(p => p.ParentFactor.Value).OrderByDescending(g => g.Sum(a => a.Hours))
                        .SelectMany(g => g.OrderByDescending(a => a.Hours)).ToList().ForEach(p => secondSeries.Points.Add(p));
                }

                var secondTotal = secondSeries.Points.Select(a => a.IsVisble).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
                foreach (var p in secondSeries.Points.Indexed())
                {
                    p.v.SetDisplayValue(secondTotal);
                    p.v.Index = p.i;
                }

                SecondSeries = secondSeries;

                Series.Points.CollectionChangedAsObservable().Subscribe(e =>
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var point in e.OldItems.OfType<PointViewModel>())
                        {
                            var removed = SecondSeries.Points.Where(p => p.ParentFactor.Equals(point.Factor)).ToList();
                            foreach (var r in removed)
                            {
                                SecondSeries.Points.Remove(r);
                                r.IsVisble.Value = false;
                            }
                        }
                    }
                    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        foreach (var point in e.NewItems.OfType<PointViewModel>())
                        {
                            var added = secondPoints.Where(p => p.ParentFactor.Equals(point.Factor)).ToList();
                            foreach (var a in added)
                            {
                                a.IsVisble.Value = true;
                                var upper = SecondSeries.Points.Indexed().FirstOrDefault(p => a.Index < p.v.Index);
                                if (upper.v != null)
                                    SecondSeries.Points.Insert(upper.i, a);
                                else
                                    SecondSeries.Points.Add(a);
                            }
                        }
                    }
                }).AddTo(myDisposables);
            }
        }
    }
}
