using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
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

        public FactorType BarXAxis { get; set; } = FactorType.Date;
        public FactorType BarCombine { get; set; } = FactorType.None;
        public FactorType BarShowTotal { get; set; } = FactorType.TopRight;
        public FactorType BarPreviousCombine { get; set; } = FactorType.None;
        public List<string> BarVisibleSeriesNames { get; set; } = new List<string>();

        public FactorType PieCombine { get; set; } = FactorType.Issue;
        public FactorType PieSort { get; set; } = FactorType.None;
        public FactorType PieShowTotal { get; set; } = FactorType.Center;
        public FactorType PieSecondCombine { get; set; } = FactorType.None;
        public FactorType PiePreviousCombine { get; set; } = FactorType.Issue;
        public List<string> PieVisiblePointNames { get; set; } = new List<string>();

        public FactorType FirstGrouping { get; set; } = FactorType.None;
        public FactorType SecondGrouping { get; set; } = FactorType.None;
        public FactorType ThirdGrouping { get; set; } = FactorType.None;

        public ChartSettingsModel()
        {
        }
    }
}
