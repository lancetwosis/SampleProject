using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize.TreeMapItems
{
    public class TicketItemViewModel : TreeMapItemViewModelBase
    {
        public ReactiveCommand GoToTicketCommand { get; set; }

        public TicketItemViewModel(PersonHourModel model) : base(model)
        {
            GoToTicketCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                System.Diagnostics.Process.Start(MyIssue.GetUrl(model.RawIssue.Id));
            }).AddTo(disposables);
        }

        public TicketItemViewModel(Issue issue) : base(issue)
        {
            GoToTicketCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                System.Diagnostics.Process.Start(MyIssue.GetUrl(issue.Id));
            }).AddTo(disposables);
        }

        public TicketItemViewModel(TicketItemViewModel parent) : this(parent.Issue)
        {
            Hours = parent.Hours;
            GoToTicketCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                System.Diagnostics.Process.Start(MyIssue.GetUrl(parent.Issue.Id));
            }).AddTo(disposables);
        }

        public override string ToString()
        {
            return $"{Hours.ToString("0.00").PadLeft(6, '0')} (Total: {TotalHours.ToString("0.00").PadLeft(6,'0')}) : {Issue.GetLongLabel()}";
        }
    }
}
