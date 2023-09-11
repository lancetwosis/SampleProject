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
    public class BarChartViewModel : ChartViewModelBase
    {
        public FactorTypeViewModel XAxisType { get; set; }
        public FactorTypeViewModel CombineType { get; set; }
        public ObservableCollection<SeriesViewModel> Serieses { get; set; }
        public TotalLabelViewModel ShowTotal { get; set; }

        private ReactiveCommand visibleAll { get; set; }
        private ReactiveCommand invisibleAll { get; set; }

        public BarChartViewModel(ResultViewModel parent) : base(ViewType.BarChart, parent)
        {
            XAxisType = new FactorTypeViewModel("X軸", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarXAxis),
                FactorType.Date, FactorType.Issue, FactorType.Project, FactorType.User, FactorType.Category, FactorType.OnTime).AddTo(disposables);
            XAxisType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            CombineType = new FactorTypeViewModel("グルーピング", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarCombine),
                FactorType.None, FactorType.Issue, FactorType.Project, FactorType.Date, FactorType.User, FactorType.Category, FactorType.OnTime).AddTo(disposables);
            CombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            Serieses = new ObservableCollection<SeriesViewModel>();

            ShowTotal = new TotalLabelViewModel(IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarShowTotal)).AddTo(disposables);

            this.factors = new List<FactorTypeViewModel>() { XAxisType, CombineType, ShowTotal };
            setupIsEdited();

            visibleAll = Serieses.AnyAsObservable(s => !s.IsVisible.Value, s => s.IsVisible).ToReactiveCommand().WithSubscribe(() =>
            {
                Serieses.ToList().ForEach(s => s.IsVisible.Value = true);
            }).AddTo(disposables);

            invisibleAll = Serieses.AnyAsObservable(s => s.IsVisible.Value, s => s.IsVisible).ToReactiveCommand().WithSubscribe(() =>
            {
                Serieses.ToList().ForEach(s => s.IsVisible.Value = false);
            }).AddTo(disposables);
        }

        public override void SetupSeries(bool needsSetVisible = true)
        {
            base.SetupSeries();

            Serieses.Clear();

            var allPoints = getAllTimeEntries();
            var allXAxises = allPoints.Select(a => a.GetFactor(XAxisType.SelectedType.Value)).Distinct().OrderBy(f => f.Value).ToList();

            if (CombineType.SelectedType.Value != FactorType.None)
            {
                var tmp = new List<SeriesViewModel>();
                foreach (var combine in allPoints.GroupBy(p => p.GetFactor(CombineType.SelectedType.Value)))
                {
                    var series = new SeriesViewModel(ViewType.BarChart, combine.Key, visibleAll, invisibleAll);
                    var xAxises = combine.GroupBy(p => p.GetFactor(XAxisType.SelectedType.Value)).ToList();
                    foreach (var xAxis in allXAxises)
                    {
                        series.Points.Add(new PointViewModel(series, xAxis, xAxises.FirstOrDefault(a => a.Key.Equals(xAxis))?.ToList()));
                    }
                    tmp.Add(series);
                }
                tmp.OrderBy(s => s.Factor.Value).ToList().ForEach(s => Serieses.Add(s));

                foreach (var group in Serieses.SelectMany(s => s.Points).GroupBy(p => p.XLabel))
                {
                    var total = group.Select(p => p.IsVisible).CombineLatest().Select(_ => group.Where(p => p.IsVisible.Value).Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
                    group.ToList().ForEach(p => p.SetDisplayValue(total));
                }
            }
            else
            {
                var series = new SeriesViewModel(ViewType.BarChart);
                var xAxises = allPoints.GroupBy(p => p.GetFactor(XAxisType.SelectedType.Value)).ToList();
                foreach (var xAxis in allXAxises)
                {
                    var point = new PointViewModel(series, xAxis, xAxises.FirstOrDefault(a => a.Key.Equals(xAxis))?.ToList());
                    point.SetDisplayValue(null);
                    series.Points.Add(point);
                }
                Serieses.Add(series);
            }

            ShowTotal.TotalHours = Serieses.SelectMany(s => s.Points.Select(p => p.IsVisible)).CombineLatest().Select(_ =>
            {
                return Serieses.Select(s => s.Points.Sum(p => p.Hours)).Sum();
            }).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            if (needsSetVisible &&
                CombineType.SelectedType.Value == parent.Model.ChartSettings.BarPreviousCombine &&
                parent.Model.ChartSettings.BarVisibleSeriesNames.Any())
            {
                foreach (var s in Serieses)
                {
                    if (parent.Model.ChartSettings.BarVisibleSeriesNames.Contains(s.Title))
                        s.IsVisible.Value = true;
                    else
                        s.IsVisible.Value = false;
                }
            }

            Serieses.Select(a => a.IsVisible).CombineLatest().Subscribe(_ =>
            {
                parent.Model.ChartSettings.BarPreviousCombine = CombineType.SelectedType.Value;
                parent.Model.ChartSettings.BarVisibleSeriesNames = Serieses.Where(a => a.IsVisible.Value).Select(a => a.Title).ToList();
            }).AddTo(myDisposables);
        }
    }
}
