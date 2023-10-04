using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.ViewModels.Visualize.Charts;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize.TreeMapItems
{
    public class GroupingItemViewModel : TreeMapItemViewModelBase
    {
        public int Depth { get; set; }

        public GroupingItemViewModel(FactorModel factor, int depth, TreeMapViewModel tree) : base(tree)
        {
            XLabel = $"{factor.Type.ToString()}: {factor.Name}";
            Depth = depth;
        }

        public override string ToString()
        {
            return $"(Total: {TotalHours.ToString("0.00").PadLeft(6, '0')}) : {XLabel}";
        }
    }
}
