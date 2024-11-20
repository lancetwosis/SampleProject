using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket.Work;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Work
{
    public class PeriodViewModel : ViewModelBase, IRequestPeriod
    {
        public ReactivePropertySlim<DateTime> StartDate { get; set; }
        public ReactivePropertySlim<DateTime> DueDate { get; set; }
        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        public PeriodViewModel(PeriodModel model)
        {
            StartDate = model.ToReactivePropertySlimAsSynchronized(m => m.StartDate).AddTo(disposables);
            DueDate = model.ToReactivePropertySlimAsSynchronized(m => m.DueDate).AddTo(disposables);
            if (StartDate.Value == default)
                resetPeriod();

            IsValid = StartDate.CombineLatest(DueDate,
                (s, d) => s <= d ? null : Resources.ReviewErrMsgInvalidTimePeriod).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        private void resetPeriod()
        {
            StartDate.Value = DateTime.Today;
            DueDate.Value = SettingsModel.Default.Calendar.GetNextWorkingDay(DateTime.Today, 3);

        }

        public void Clear()
        {
            resetPeriod();
        }
    }
}