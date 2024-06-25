using LibRedminePower.Enums;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings
{
    public class SubIssueSettingModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public bool IsEnabled { get; set; } = true;
        public int Order { get; set; }
        public int TrackerId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public StringCompareType SubjectCompare { get; set; } = StringCompareType.StartWith;
        public ObservableCollection<int> StatusIds { get; set; } = new ObservableCollection<int>();
        public StatusCompareType StatusCompare { get; set; } = StatusCompareType.Equals;

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Title)) return false;
                if (TrackerId <= 0 && string.IsNullOrEmpty(Subject)) return false;
                return true;
            }
        }

        public bool IsMatch(Issue issue)
        {
            
            if (TrackerId == TicketFields.Standard.Tracker.NOT_SPECIFIED.Id || issue.Tracker.Id == TrackerId)
            {
                if (!string.IsNullOrEmpty(Subject) && SubjectCompare.IsMatch(issue.Subject, Subject))
                {
                    if (StatusIds.Any())
                    {
                        if (StatusCompare.IsMatch(issue.Status.Id, StatusIds))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
