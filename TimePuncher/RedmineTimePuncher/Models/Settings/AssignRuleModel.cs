using LibRedminePower.Enums;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.Settings
{
    public class AssignRuleModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public List<int> ProjectIds { get; set; }
        public List<int> TrackerIds { get; set; }
        public string Subject { get; set; }
        public StringCompareType StringCompare { get; set; }
        public List<int> StatusIds { get; set; }
        public Enums.AssignToType AssignTo { get; set; }

        public AssignRuleModel()
        {
            ProjectIds = new List<int>();
            TrackerIds = new List<int>();
            StatusIds = new List<int>();
        }
    }
}