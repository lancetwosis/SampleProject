﻿using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
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

        public GroupingItemViewModel(FactorModel factor, int depth) : base()
        {
            XLabel = $"{factor.Type.ToString()}: {factor.Name}";
            Depth = depth;
        }
    }
}
