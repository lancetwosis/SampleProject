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
    public class SpentHours : MyTicketFieldBase<double?>
    {
        public static double MAX { get; set; }

        public double Max => MAX;

        public SpentHours(Issue issue)
        {
            if (issue == null || !issue.SpentHours.HasValue)
                return;

            this.ObserveProperty(a => a.Value).SubscribeWithErr(v =>
            {
                if (v.HasValue && v.Value > MAX)
                    MAX = v.Value;
            }).AddTo(disposables);

            Value = issue.SpentHours.Value;
        }
    }
}
