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

        private ResultViewModel parent { get; set; }

        public BarChartViewModel(ResultViewModel parent) : base(ViewType.BarChart, parent)
        {
            this.parent = parent;

            XAxisType = new FactorTypeViewModel("X軸", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarXAxis),
                FactorType.Date, FactorType.Issue, FactorType.Project, FactorType.User, FactorType.Category, FactorType.OnTime).AddTo(disposables);
            XAxisType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());
            CombineType = new FactorTypeViewModel("グルーピング", IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarCombine),
                FactorType.None, FactorType.Issue, FactorType.Project, FactorType.Date, FactorType.User, FactorType.Category, FactorType.OnTime).AddTo(disposables);
            CombineType.SelectedType.Skip(1).Subscribe(_ => SetupSeries());

            Serieses = new ObservableCollection<SeriesViewModel>();

            ShowTotal = new TotalLabelViewModel(IsEnabled, parent.Model.ChartSettings.ToReactivePropertySlimAsSynchronized(a => a.BarShowTotal)).AddTo(disposables);
        }

        public override void SetupSeries()
        {
            base.SetupSeries();

            Serieses.Clear();

            var allPoints = parent.Tickets.SelectMany(t => t.GetAllTimeEntries()).ToList();
            var allXAxises = allPoints.Select(a => a.GetFactor(XAxisType.SelectedType.Value)).Distinct().OrderBy(f => f.Value).ToList();

            ShowTotal.TotalHours = allPoints.Sum(p => p.TotalHours);

            if (CombineType.SelectedType.Value != FactorType.None)
            {
                var tmp = new List<SeriesViewModel>();
                foreach (var combine in allPoints.GroupBy(p => p.GetFactor(CombineType.SelectedType.Value)))
                {
                    var series = new SeriesViewModel(ViewType.BarChart, combine.Key);
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
                    var total = group.Select(p => p.IsVisble).CombineLatest().Select(_ => group.Where(p => p.IsVisble.Value).Sum(p => p.Hours)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
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
        }
    }
}
