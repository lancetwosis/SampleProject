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

        public PieChartViewModel(ResultViewModel parent) : base(ViewType.PieChart, parent)
        {
            CombineType = new FactorTypeViewModel(Resources.VisualizeFactorGrouping1, IsEnabled,
                parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieCombine), FactorTypes.GetGroupings()).AddTo(disposables);
            CombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            SecondCombineType = new FactorTypeViewModel(Resources.VisualizeFactorGrouping2, IsEnabled,
                parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieSecondCombine), FactorTypes.Get2ndGroupings()).AddTo(disposables);
            SecondCombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            ShowSecondSeries = SecondCombineType.SelectedType.Select(a => a != FactorTypes.None).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SortType = new FactorTypeViewModel(Resources.VisualizeFactorSort, IsEnabled,
                parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieSort), FactorTypes.GetSortDirections()).AddTo(disposables);
            SortType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            ShowTotal = new TotalLabelViewModel(IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.PieShowTotal)).AddTo(disposables);

            this.factors = new List<FactorTypeViewModel>() { CombineType, SecondCombineType, SortType, ShowTotal };
            setupIsEdited();
        }

        public override void SetupSeries(bool needsSetVisible = true)
        {
            base.SetupSeries();

            var allPoints = getAllTimeEntries();

            var series = new SeriesViewModel(ViewType.PieChart);
            var points = allPoints.GroupBy(a => a.GetFactor(CombineType.SelectedType.Value))
                .Select(a => new PointViewModel(series, a.Key, a.ToList()))
                .ToList();

            if (SortType.SelectedType.Value.Equals(FactorTypes.None))
            {
                points.OrderBy(p => p.Factor.Value).ToList().ForEach(p => series.Points.Add(p));
            }
            else if (SortType.SelectedType.Value.Equals(FactorTypes.ASC))
            {
                points.OrderBy(p => p.Hours).ToList().ForEach(p => series.Points.Add(p));
            }
            else if (SortType.SelectedType.Value.Equals(FactorTypes.DESC))
            {
                points.OrderByDescending(p => p.Hours).ToList().ForEach(p => series.Points.Add(p));
            }

            var total =  series.Points.Select(a => a.IsVisible).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            foreach (var p in series.Points.Indexed())
            {
                p.v.SetDisplayValue(total);
                p.v.Index = p.i;
            }

            Series = series;

            LegendItems = new LegendItemCollection();
            points.OrderBy(p => p.Factor.Value).Select(p => p.ToLegendItem()).ToList().ForEach(i => LegendItems.Add(i));

            // 表示非表示が切り替わったら合計の再計算
            ShowTotal.TotalHours = series.Points.Select(p => p.IsVisible).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            // PointViewModel の IsVisible.Subscribe により Series.Points には表示されているもののみが含まれる
            Series.VisibleAllCommand = Series.Points.CollectionChangedAsObservable()
                .Select(_ => Series.Points.Count < points.Count).ToReactiveCommand().WithSubscribe(() =>
            {
                points.ToList().ForEach(p => p.IsVisible.Value = true);
            }).AddTo(myDisposables);
            Series.InvisibleAllCommand = Series.Points.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                Series.Points.ToList().ForEach(p => p.IsVisible.Value = false);
            }).AddTo(myDisposables);

            if (!SecondCombineType.SelectedType.Value.Equals(FactorTypes.None))
            {
                var secondSeries = new SeriesViewModel(ViewType.PieChart);
                var secondPoints = allPoints.GroupBy(a => (a.GetFactor(SecondCombineType.SelectedType.Value), a.GetFactor(CombineType.SelectedType.Value)))
                    .Select(a => new PointViewModel(secondSeries, a.Key.Item1, a.Key.Item2, a.ToList()))
                    .ToList();

                if (SortType.SelectedType.Value.Equals(FactorTypes.None))
                {
                    secondPoints.OrderBy(p => p.ParentFactor.Value).ThenBy(p => p.Factor.Value).ToList().ForEach(p => secondSeries.Points.Add(p));
                }
                else if (SortType.SelectedType.Value.Equals(FactorTypes.ASC))
                {
                    secondPoints.GroupBy(p => p.ParentFactor.Value).OrderBy(g => g.Sum(a => a.Hours))
                        .SelectMany(g => g.OrderBy(a => a.Hours)).ToList().ForEach(p => secondSeries.Points.Add(p));
                }
                else if (SortType.SelectedType.Value.Equals(FactorTypes.DESC))
                {
                    secondPoints.GroupBy(p => p.ParentFactor.Value).OrderByDescending(g => g.Sum(a => a.Hours))
                        .SelectMany(g => g.OrderByDescending(a => a.Hours)).ToList().ForEach(p => secondSeries.Points.Add(p));
                }

                var secondTotal = secondSeries.Points.Select(a => a.IsVisible).CombineLatest().Select(_ => series.Points.Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
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
                                r.IsVisible.Value = false;
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
                                a.IsVisible.Value = true;
                            }
                        }
                    }
                }).AddTo(myDisposables);
            }

            // 凡例の表示非表示チェックボックスの復元
            if (needsSetVisible &&
                CombineType.SelectedType.Value == parent.Model.ChartSettings.PiePreviousCombine &&
                parent.Model.ChartSettings.PieVisiblePointNames.Any())
            {
                foreach (var p in Series.Points.ToList())
                {
                    if (parent.Model.ChartSettings.PieVisiblePointNames.Contains(p.Factor.Name))
                        p.IsVisible.Value = true;
                    else
                        p.IsVisible.Value = false;
                }
            }

            // 凡例の表示非表示チェックボックスの保存
            // PointViewModel の IsVisible.Subscribe により Series.Points には表示されているもののみが含まれる
            Series.Points.CollectionChangedAsObservable().StartWithDefault().Subscribe(_ =>
            {
                parent.Model.ChartSettings.PiePreviousCombine = CombineType.SelectedType.Value;
                parent.Model.ChartSettings.PieVisiblePointNames = Series.Points.Select(a => a.Factor.Name).ToList();
            }).AddTo(myDisposables);
        }

        public void SetCustomFieldFactors(List<FactorType> customFieldFactors)
        {
            foreach (var f in customFieldFactors)
            {
                CombineType.Types.Add(f);
                SecondCombineType.Types.Add(f);
            }
        }
    }
}
