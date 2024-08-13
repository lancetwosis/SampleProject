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
            : base(issue, redmine, settings)
        {
            TargetSubTicket = target;
        }

        public override void SetIssue(Issue issue)
        {
            base.SetIssue(issue);

            if (issue == null)
                return;

            if (settings.SubIssues.Properties.Any(p => p.MyField.HasValue &&
                (p.MyField.Value == Enums.MyIssuePropertyType.MySpentHours ||
                 p.MyField.Value == Enums.MyIssuePropertyType.DiffEstimatedSpent)))
            {
                var _ = Task.Run(() =>
                {
                    timeEntries = redmine.GetTimeEntries(issue.Id);
                    RaisePropertyChanged(nameof(MySpentHours));
                    RaisePropertyChanged(nameof(DiffEstimatedSpent));
                });
            }

            if (settings.SubIssues.Properties.Any(p => p.MyField.HasValue &&
                p.MyField.Value == Enums.MyIssuePropertyType.ReplyCount))
            {
                getReplyCount();
            }

            // アサインが変更された場合は、合計時間の変更通知を行う。
            this.ObserveProperty(a => a.AssignedTo.Value).SubscribeWithErr(_ => RaisePropertyChanged(nameof(MySpentHours))).AddTo(disposables);
            this.ObserveProperty(a => a.EstimatedHours.Value).SubscribeWithErr(_ => RaisePropertyChanged(nameof(DiffEstimatedSpent))).AddTo(disposables);
        }
    }
}