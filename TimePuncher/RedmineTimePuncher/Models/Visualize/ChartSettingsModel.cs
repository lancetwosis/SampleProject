using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize
{
    public class ChartSettingsModel : LibRedminePower.Models.Bases.ModelBase
    {
        public ViewType ViewType { get; set; } = ViewType.BarChart;

        public FactorType BarXAxis { get; set; } = FactorTypes.Date;
        public FactorType BarCombine { get; set; } = FactorTypes.None;
        public FactorType BarSort { get; set; } = FactorTypes.None;
        public FactorType BarShowTotal { get; set; } = FactorTypes.TopRight;
        public FactorType BarPreviousCombine { get; set; } = FactorTypes.None;
        public List<string> BarVisibleSeriesNames { get; set; } = new List<string>();

        public FactorType PieCombine { get; set; } = FactorTypes.Issue;
        public FactorType PieSort { get; set; } = FactorTypes.None;
        public FactorType PieShowTotal { get; set; } = FactorTypes.Center;
        public FactorType PieSecondCombine { get; set; } = FactorTypes.None;
        public FactorType PiePreviousCombine { get; set; } = FactorTypes.Issue;
        public List<string> PieVisiblePointNames { get; set; } = new List<string>();

        public FactorType FirstGrouping { get; set; } = FactorTypes.None;
        public FactorType SecondGrouping { get; set; } = FactorTypes.None;
        public FactorType ThirdGrouping { get; set; } = FactorTypes.None;

        public ChartSettingsModel()
        {
        }
    }
}
