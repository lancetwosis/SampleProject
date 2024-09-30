using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using LibRedminePower.Extentions;
using System.Windows.Data;
using RedmineTableEditor.Models.Bases;
using System.ComponentModel.DataAnnotations;
using RedmineTableEditor.Models.FileSettings;

namespace RedmineTableEditor.Models
{
    public class MySubIssue : MyIssueBase
    {
        public SubIssueSettingModel TargetSubTicket { get; set; }

        public MySubIssue(RedmineManager redmine, SubIssueSettingModel target, Issue issue, FileSettingsModel settings)
            : base(issue, redmine, settings.SubIssues.Properties)
        {
            TargetSubTicket = target;
        }
    }
}