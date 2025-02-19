using LibRedminePower.Extentions;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.MyTicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Bases;
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

namespace RedmineTableEditor.Models.MyTicketFields
{
    public class ReplyCount : MyTicketFieldBase<int?>
    {
        public ReplyCount(Issue issue, RedmineManager redmine)
        {
            if (issue == null || issue.Journals == null)
                return;

            var assignJournals = issue.Journals.Select(j => j.Details?.FirstOrDefault(d => d.Name == "assigned_to_id"))
                                               .Where(a => a != null).ToList();
            Value = assignJournals.Count;
            if (Value == 0)
                return;

            var sb = new StringBuilder();
            if (assignJournals[0].OldValue != null)
                sb.AppendLine($"{redmine.Users.First(u => u.Id.ToString() == assignJournals[0].OldValue).Name}");
            else
                sb.AppendLine(Properties.Resources.UserNoAssignee);
            var assignees = assignJournals.Select(d =>
            {
                if (d.NewValue == null)
                    return Properties.Resources.UserNoAssignee;

                var user = redmine.Users.FirstOrDefault(u => u.Id.ToString() == d.NewValue);
                return user != null ? user.Name : Properties.Resources.UserInvalid;
            }).ToList();
            sb.Append(string.Join(Environment.NewLine, assignees.Select(a => $"  > {a}")));
            ToolTip = sb.ToString();
        }
    }
}
