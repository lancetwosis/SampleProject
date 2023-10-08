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
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.Properties;
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
        public FactorTypeViewModel SortType { get; set; }
        public ObservableCollection<SeriesViewModel> Serieses { get; set; }
        public TotalLabelViewModel ShowTotal { get; set; }

        private ReactiveCommand visibleAll { get; set; }
        private ReactiveCommand invisibleAll { get; set; }

        public BarChartViewModel(ResultViewModel parent) : base(ViewType.BarChart, parent)
        {
            XAxisType = new FactorTypeViewModel(Resources.VisualizeFactorXAxis, IsEnabled,
                parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarXAxis), FactorTypes.GetGroupings()).AddTo(disposables);
            XAxisType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            CombineType = new FactorTypeViewModel(Resources.VisualizeFactorGrouping, IsEnabled,
                parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarCombine), FactorTypes.Get2ndGroupings()).AddTo(disposables);
            CombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            SortType = new FactorTypeViewModel(Resources.VisualizeFactorSort,
                IsEnabled.CombineLatest(XAxisType.IsContinuous, (ie, ic) => ie && !ic).ToReadOnlyReactivePropertySlim().AddTo(disposables),
                parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarSort), FactorTypes.GetSortDirections()).AddTo(disposables);
            SortType.SelectedType.Skip(1).Subscribe(_ => reorderXLabels()).AddTo(disposables);

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

        private void reorderXLabels()
        {
            if (XAxisType.IsContinuous.Value || Serieses.Count == 0)
                return;

            List<FactorModel> sortedLabels = null;
            if (SortType.SelectedType.Value.Equals(FactorTypes.None))
                sortedLabels = Serieses[0].Points.OrderBy(p => p.Factor.Value).Select(p => p.Factor).ToList();
            else if (SortType.SelectedType.Value.Equals(FactorTypes.ASC))
                sortedLabels = Serieses[0].Points.OrderBy(p => p.TotalHours.Value).Select(p => p.Factor).ToList();
            else if (SortType.SelectedType.Value.Equals(FactorTypes.DESC))
                sortedLabels = Serieses[0].Points.OrderByDescending(p => p.TotalHours.Value).Select(p => p.Factor).ToList();

            foreach (var s in Serieses)
            {
                s.OrderPoints(sortedLabels);
            }
        }

        public override void SetupSeries(bool needsSetVisible = true)
        {
            base.SetupSeries();

            Serieses.Clear();

            var allPoints = getAllTimeEntries();
            var allXAxises = allPoints.Select(a => a.GetFactor(XAxisType.SelectedType.Value)).Distinct().OrderBy(f => f.Value).ToList();

            if (!CombineType.SelectedType.Value.Equals(FactorTypes.None))
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

            // 表示非表示が切り替わったら合計の再計算
            ShowTotal.TotalHours = Serieses.Select(s => s.IsVisible).CombineLatest().Select(_ =>
            {
                return Serieses.Select(s => s.Points.Where(p => p.IsVisible.Value).Sum(p => p.Hours)).Sum();
            }).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            // 表示非表示が切り替わったら並べ替え
            Serieses.Select(s => s.IsVisible).CombineLatest().Subscribe(_ => reorderXLabels()).AddTo(myDisposables);

            // 凡例の表示非表示チェックボックスの復元
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

            // 凡例の表示非表示チェックボックスの保存
            Serieses.Select(a => a.IsVisible).CombineLatest().Subscribe(_ =>
            {
                parent.Model.ChartSettings.BarPreviousCombine = CombineType.SelectedType.Value;
                parent.Model.ChartSettings.BarVisibleSeriesNames = Serieses.Where(a => a.IsVisible.Value).Select(a => a.Title).ToList();
            }).AddTo(myDisposables);
        }

        public void SetCustomFieldFactors(List<FactorType> customFieldFactors)
        {
            foreach (var f in customFieldFactors)
            {
                XAxisType.Types.Add(f);
                CombineType.Types.Add(f);
            }
        }
    }
}
