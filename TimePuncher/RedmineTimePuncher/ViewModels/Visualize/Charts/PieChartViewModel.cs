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

            ShowTotal.TotalHours = allPoints.Sum(p => p.TotalHours);

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

            var total =  series.Points.Select(a => a.IsVisble).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            foreach (var p in series.Points)
            {
                p.SetDisplayValue(total);
            }

            Series = series;

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
                //var secondTotal = secondSeries.Points.Sum(p => p.Hours);
                foreach (var p in secondSeries.Points)
                {
                    p.SetDisplayValue(secondTotal);
                }

                SecondSeries = secondSeries;
            }
        }
    }
}
