using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Microsoft.VisualBasic.CompilerServices;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.Models.Settings
{
    public class CategorySettingModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public int Id { get; set; }
        public string Name => TimeEntry.Name;
        public bool IsEnabled { get; set; } = true;
        private System.Drawing.Color color;
        public System.Drawing.Color Color
        {
            get { return color; }
            set 
            { 
                color = value;

                // ModelをCloneしている場合、PropertyChangedを刈り取る方法だと
                // Clone後のModelで機能しなかったため、ここで実装している。
                ForeColor = color.ToTextColor();
            }
        }
        public System.Drawing.Color ForeColor { get; set; }
        public bool IsBold { get; set; } = false;
        public bool IsItalic { get; set; } = false;
        public Enums.AndOrType AndOrType { get; set; }
        public int Order { get; set; }
        public TimeEntryActivity TimeEntry { get; set; }
        public List<MyTracker> TargetTrackers { get; set; }
        public ObservableCollection<AssignRuleModel> Rules { get; set; }
        public bool IsWorkingTime { get; set; } = true;

        public CategorySettingModel()
        {
            TargetTrackers = new List<MyTracker>();
            Rules = new ObservableCollection<AssignRuleModel>();
        }

        public CategorySettingModel(TimeEntryActivity timeEntry) : this()
        {
            Id = timeEntry.Id;
            Color = ColorEx.GetRandColor(timeEntry.Name);

            Setup(timeEntry);
        }

        public void Setup(TimeEntryActivity timeEntry)
        {
            TimeEntry = timeEntry;
        }

        /// <summary>
        /// 引数には、対象のチケットを含み、その親チケットを再帰的に取得したリストを指定すること。
        /// その際、リストは親チケット→子チケットの順でソートされていること。
        /// </summary>
        public bool IsMatch(IEnumerable<MyIssue> issues, int myUserId)
        {
            if (Rules == null || !Rules.Any())
                return false;

            if (TargetTrackers.Any())
            {
                issues = issues.Where(i => TargetTrackers.Contains(i.Tracker));
                if (!issues.Any())
                    return false;
            }

            var results = Rules.Select(rule =>
            {
                foreach (var issue in issues.Reverse())
                {
                    if (!rule.ProjectIds.Any() || rule.ProjectIds.Contains(issue.Project.Id))
                    {
                        if (!rule.TrackerIds.Any() || rule.TrackerIds.Contains(issue.Tracker.Id))
                        {
                            if (!rule.StatusIds.Any() || rule.StatusIds.Contains(issue.Status.Id))
                            {
                                if (string.IsNullOrEmpty(rule.Subject) || rule.StringCompare.IsMatch(issue.Subject, rule.Subject))
                                {
                                    switch (rule.AssignTo)
                                    {
                                        case Enums.AssignToType.Any:
                                            return true;
                                        case Enums.AssignToType.Me:
                                            return issue.AssignedTo.Id == myUserId;
                                        case Enums.AssignToType.NotMe:
                                            return issue.AssignedTo.Id != myUserId;
                                        default:
                                            throw new InvalidOperationException();
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            });

            return AndOrType == Enums.AndOrType.And ? results.All(a => a) : results.Any(a => a);
        }

        public bool IsTergetTracker(MyTracker tracker)
        {
            return TargetTrackers.Any() ? TargetTrackers.Contains(tracker) : true;
        }

        public Brush GetBackground()
        {
            return new SolidColorBrush(Color.ToMediaColor());
        }
    }
}
