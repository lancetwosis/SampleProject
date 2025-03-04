﻿using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Factors;
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
using Telerik.Windows.Controls.Legend;

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
        public string Url { get; set; }
        public int Index { get; set; }

        public ReactivePropertySlim<bool> IsVisible { get; set; }

        /// <summary>
        /// このポイントが属する集団の Hours の合計。BarChart の場合、同じ XLabel のポイントの合計。
        /// 現状、BarChart のみで使用。
        /// </summary>
        public ReadOnlyReactivePropertySlim<double> TotalHours { get; set; }

        public ReactiveCommand VisibleAllCommand => series.VisibleAllCommand;
        public ReactiveCommand InvisibleAllCommand => series.InvisibleAllCommand;

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
            {
                Color = series.Color;
                IsVisible = series.IsVisible;
            }
            else if (series.Type == ViewType.PieChart)
            {
                Color = factor.GetColor();
                IsVisible = new ReactivePropertySlim<bool>(true);
                IsVisible.Skip(1).SubscribeWithErr(_ =>
                {
                    var i = this.series.Points.IndexOf(this);
                    if (i >= 0)
                    {
                        this.series.Points.Remove(this);
                    }
                    else
                    {
                        var upper = this.series.Points.Indexed().FirstOrDefault(p => this.Index < p.v.Index);
                        if (upper.v != null)
                            this.series.Points.Insert(upper.i, this);
                        else
                            this.series.Points.Add(this);
                    }
                }).AddTo(disposables);
            }

            if (factor.Type.Equals(FactorTypes.Date))
                XDateTime = (DateTime) factor.RawValue;
            if (factor.Type.Equals(FactorTypes.Issue))
                Url = MyIssue.GetUrl((factor.RawValue as Issue).Id);
        }

        /// <summary>
        /// PieChart の第二チャート用のコンストラクタ
        /// </summary>
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
                var label = Factor.Type.Equals(FactorTypes.Issue) ? (Factor.RawValue as Issue).GetFullLabel() : XLabel;
                var seriesName = (series.Factor != null && series.Factor.Type.Equals(FactorTypes.Issue)) ? (series.Factor.RawValue as Issue).GetFullLabel() : series.Title;
                TotalHours = total != null ? total : new ReactivePropertySlim<double>(Hours).ToReadOnlyReactivePropertySlim();
                ToolTip = new ToolTipViewModel(seriesName, label, Hours, TotalHours).AddTo(disposables);
            }
            else if (series.Type == ViewType.PieChart)
            {
                var label = Factor.Type.Equals(FactorTypes.Issue) ? (Factor.RawValue as Issue).GetFullLabel() : Factor.Name;
                ToolTip = new ToolTipViewModel(label, Hours, total, true);
                ToolTip.Percentage.SubscribeWithErr(per =>
                {
                    DisplayValue = string.Join(Environment.NewLine, new[]
                    {
                        $"{Factor.Name}",
                        $"  {Hours} h ({per} %)",
                    });
                });
            }
        }

        public LegendItem ToLegendItem()
        {
            var item = new LegendItem() { MarkerFill = Color, Title = XLabel, Presenter = this };
            if (Factor.Type.Equals(FactorTypes.Issue))
            {
                item.Title = (Factor.RawValue as Issue).GetFullLabel();
            }

            return item;
        }

        public override string ToString()
        {
            return $"({XLabel}, {Hours})";
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
                Percentage = totalHours.Select(a => $"{hours / a * 100:F1}").ToReadOnlyReactivePropertySlim().AddTo(disposables);
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
