using LibRedminePower.Extentions;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.MyTicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Properties;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Views;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTableEditor.Models.MyTicketFields
{
    public class DaysUntilCreated : MyTicketFieldBase<double?>
    {
        public DaysUntilCreated(Issue issue, RedmineManager redmine)
        {
            if (issue == null || !issue.CreatedOn.HasValue)
                return;

            var parent = issue.ParentIssue != null ? redmine.GetIssueFromCache(issue.ParentIssue.Id, false) : null;
            if (parent == null)
            {
                ToolTip = Resources.DaysUntilCreatedParentNotExist;
                return;
            }

            Value = (issue.CreatedOn.Value - parent.CreatedOn.Value).TotalDays;
            ToolTip = string.Format(Resources.DaysUntilCreatedParent, $"#{parent.Id}", parent.Subject, parent.CreatedOn.Value.ToString("yy/MM/dd hh:mm"));
        }
    }
}
