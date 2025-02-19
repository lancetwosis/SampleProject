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
    public class RequiredDays : MyTicketFieldBase<double?>
    {
        public RequiredDays(Issue issue, RedmineManager redmine)
        {
            if (issue == null || !issue.CreatedOn.HasValue)
                return;

            var status = redmine.Cache.Statuss.FirstOrDefault(s => s.Id == issue.Status.Id);
            if (status != null && status.IsClosed && issue.ClosedOn.HasValue)
            {
                Value = (issue.ClosedOn.Value - issue.CreatedOn.Value).TotalDays;
            }
            else
            {
                var now = DateTime.Now;
                Value = (now - issue.CreatedOn.Value).TotalDays;
                ToolTip = string.Format(Resources.RequiredDaysNotClosed, now.ToString("yy/MM/dd HH:mm"));
                FontStyle = FontStyles.Oblique;
            }
        }
    }
}
