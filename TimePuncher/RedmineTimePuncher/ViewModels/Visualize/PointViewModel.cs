using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class PointViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public double Hours { get; set; }
        public string DisplayValue { get; set; }
        public string XLabel { get; set; }
        public Brush Color { get; set; }
        public ToolTipViewModel ToolTip { get; set; }
        public DateTime XDateTime { get; set; }

        public ReactivePropertySlim<bool> IsVisble => series.IsVisble;

        public FactorModel Factor { get; set; }
        public FactorModel ParentFactor { get; set; }

        private SeriesViewModel series { get; set; }

        public PointViewModel(SeriesViewModel series, FactorModel factor, List<PersonHourModel> models = null)
        {
            this.series = series;

            Factor = factor;
            XLabel = factor.Name;
            Hours = models != null ? models.Sum(m => m.TotalHours) : 0;

            if (series.Type == ViewType.BarChart)
                Color = series.Color;
            else if (series.Type == ViewType.PieChart)
                Color = factor.GetColor();

            if (factor.Type == FactorType.Date)
                XDateTime = (DateTime) factor.Value;
        }

        public PointViewModel(SeriesViewModel series, FactorModel factor, FactorModel parentFactor, List<PersonHourModel> models = null)
            : this(series, factor, models)
        {
            ParentFactor = parentFactor;
        }

        protected PointViewModel()
        {
        }

        public void SetDisplayValue(ReadOnlyReactivePropertySlim<double> total)
        {
            if (series.Type == ViewType.BarChart)
            {
                var label = Factor.Type == FactorType.Issue ? (Factor.RawValue as Issue).GetFullLabel() : XLabel;
                var seriesName = series.Factor?.Type == FactorType.Issue ? (series.Factor.RawValue as Issue).GetFullLabel() : series.Title;
                ToolTip = new ToolTipViewModel(seriesName, label, Hours, total).AddTo(disposables);
            }
            else if (series.Type == ViewType.PieChart)
            {
                var label = Factor.Type == FactorType.Issue ? (Factor.RawValue as Issue).GetFullLabel() : Factor.Name;
                var per = Hours / total.Value * 100;
                ToolTip = new ToolTipViewModel(label, Hours, total, true);
                DisplayValue = string.Join(Environment.NewLine, new[]
                {
                    $"{Factor.Name}",
                    $"  {Hours} h ({per:F1} %)",
                });
            }
        }
    }

    public class ToolTipViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string Label { get; set; }
        public string SeriesName { get; set; }
        public string Hours { get; set; }
        public bool ShowTotal { get; set; }
        public ReadOnlyReactivePropertySlim<string> TotalHours { get; set; }
        public ReadOnlyReactivePropertySlim<string> Percentage { get; set; }

        public ToolTipViewModel(string label, double hours)
        {
            Label = label;
            Hours = hours.ToString();
        }

        public ToolTipViewModel(string label, double hours, ReadOnlyReactivePropertySlim<double> totalHours, bool needsPercentage = false)
            : this(label, hours)
        {
            if (totalHours != null)
            {
                TotalHours = totalHours.Select(a => a.ToString()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
                ShowTotal = true;
            }

            if (needsPercentage)
            {
                Percentage = totalHours.Select(a => $"{hours / a * 100:F2}").ToReadOnlyReactivePropertySlim().AddTo(disposables);
            }
        }

        public ToolTipViewModel(string label, double hours, double totalHours, bool needsPercentage = false)
            : this (label, hours, new ReactivePropertySlim<double>(totalHours).ToReadOnlyReactivePropertySlim(), needsPercentage)
        {
        }

        public ToolTipViewModel(string seriesName, string label, double hours, ReadOnlyReactivePropertySlim<double> totalHours)
            : this (label, hours, totalHours, false)
        {
            SeriesName = seriesName;
        }

    }
}
