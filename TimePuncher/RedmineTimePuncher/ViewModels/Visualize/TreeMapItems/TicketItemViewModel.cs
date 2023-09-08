using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.ViewModels.Visualize.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize.TreeMapItems
{
    public class TicketItemViewModel : TreeMapItemViewModelBase
    {
        public ReactiveCommand<int> GoToTicketCommand { get; set; }
        public ReactiveCommand<int> ExpandTicketCommand { get; set; }
        public ReactiveCommand<int> CollapseTicketCommand { get; set; }
        public ReactiveCommand<int> RemoveTicketCommand { get; set; }

        private TicketItemViewModel(Issue issue, double hours, string xLabel, TreeMapViewModel tree) : base()
        {
            Issue = issue;
            Hours = hours;
            XLabel = xLabel;

            GoToTicketCommand = tree.GoToTicketCommand;
            ExpandTicketCommand = tree.ExpandCommand;
            CollapseTicketCommand = tree.CollapseCommand;
            RemoveTicketCommand = tree.RemoveCommand;
        }

        public TicketItemViewModel(PersonHourModel model, TreeMapViewModel tree)
            : this(model.RawIssue, model.TotalHours, model.RawIssue.GetLabel(), tree)
        {
        }

        /// <summary>
        /// 作業時間が設定されていない場合のコンストラクタ
        /// </summary>
        public TicketItemViewModel(Issue issue, TreeMapViewModel tree)
            : this(issue, 0, issue.GetLabel(), tree)
        {
        }

        /// <summary>
        /// 自分自身に作業時間が設定されていて、子チケットもある場合のダミー用
        /// </summary>
        public TicketItemViewModel(TicketItemViewModel parent, TreeMapViewModel tree)
            : this(parent.Issue, parent.Hours, parent.Issue.GetLabel(), tree)
        {
        }

        public override string ToString()
        {
            return $"{Hours.ToString("0.00").PadLeft(6, '0')} (Total: {TotalHours.ToString("0.00").PadLeft(6,'0')}) : {Issue.GetLongLabel()}";
        }
    }
}
