using LibRedminePower.Extentions;
using LibRedminePower.Models.Bases;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.MyTicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Standard;
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
    public class DiffEstimatedSpent : MyTicketFieldBase<double?>
    {
        public static double MAX { get; set; }

        public double Max => MAX;

        public DiffEstimatedSpent(Issue issue, EstimatedHours estimatedHours, MySpentHours mySpentHours, Status status)
        {
            if (issue == null)
                return;

            this.ObserveProperty(a => a.Value).SubscribeWithErr(v =>
            {
                if (v.HasValue && v.Value > MAX)
                    MAX = v.Value;
            }).AddTo(disposables);

            estimatedHours.ObserveProperty(a => a.Value).CombineLatest(
                mySpentHours.ObserveProperty(a => a.Value), status.ObserveProperty(a => a.SelectedItem),
                (estimated, my, s) => (estimated, my, s)).SubscribeWithErr(p =>
            {
                if (!p.estimated.HasValue && !p.my.HasValue)
                {
                    Value = null;
                    return;
                }

                var estimated = p.estimated.HasValue ? p.estimated.Value : 0;
                var spent = p.my.HasValue ? p.my.Value : 0;
                var diff = estimated - spent;
                if (diff <= 0 || p.s.IsClosed)
                    diff = 0;

                Value = diff;
            }).AddTo(disposables);
        }
    }
}
