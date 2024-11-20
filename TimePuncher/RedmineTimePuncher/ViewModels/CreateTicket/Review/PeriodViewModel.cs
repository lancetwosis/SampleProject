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
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class PeriodViewModel : 
        SavableCustomFieldViewModelBase<ReviewMethodSettingModel, ReviewMethodCustomField, ReviewMethodValue, PeriodModel>,
        IRequestPeriod
    {
        public ReactivePropertySlim<DateTime> StartDate { get; set; }
        public ReactivePropertySlim<DateTime> DueDate { get; set; }
        public ReactivePropertySlim<DateTime> StartDateTime { get; set; }
        public ReactivePropertySlim<DateTime> DueDateTime { get; set; }

        public ReadOnlyReactivePropertySlim<bool> NeedsOnDesktop { get; set; }
        public ReadOnlyReactivePropertySlim<bool> NeedsFaceToFace { get; set; }
        public ReadOnlyReactivePropertySlim<bool> NeedsOutlookIntegration { get; set; }

        public ReactivePropertySlim<bool> NeedsCreateOutlookAppointment { get; set; }

        public PeriodViewModel(PeriodModel model)
            : base(Resources.ReviewReviewMethod, model, m => m.Method, s => s.CreateTicket.ReviewMethod)
        {
            StartDate = model.ToReactivePropertySlimAsSynchronized(m => m.StartDate).AddTo(disposables);
            DueDate = model.ToReactivePropertySlimAsSynchronized(m => m.DueDate).AddTo(disposables);
            StartDateTime = model.ToReactivePropertySlimAsSynchronized(m => m.StartDateTime).AddTo(disposables);
            DueDateTime = model.ToReactivePropertySlimAsSynchronized(m => m.DueDateTime).AddTo(disposables);
            if (StartDate.Value == default)
                resetPeriod();

            NeedsCreateOutlookAppointment = model.ToReactivePropertySlimAsSynchronized(m => m.NeedsCreateOutlookAppointment).AddTo(disposables);

            var isEnabled = Setting.SelectMany(s => s.ObserveProperty(a => a.IsEnabled)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var needsOnDesktop = SelectedValue.Select(v => v?.NeedsOnDesktop ?? true).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var needsFaceToFace = SelectedValue.Select(v => v?.NeedsFaceToFace ?? false).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // 「期間」の選択は以下の条件で表示する
            // 「レビュー方法の指定」が無効 or 机上レビューが有効 or 机上、対面のどちらも無効
            NeedsOnDesktop = isEnabled.CombineLatest(needsOnDesktop, needsFaceToFace,
                (i, nd, nf) => !i || nd || (!nd && !nf)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // 「開催日時」の選択は、「レビュー方法の指定」が有効 and 対面レビューが有効の場合のみ、表示する
            NeedsFaceToFace = isEnabled.CombineLatest(needsFaceToFace, (i, nf) => i && nf).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            var validValue = Setting.CombineLatest(SelectedValue, (s, v) => s.IsEnabled ? validate() : null)
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var validPeriod = new[]
            {
                StartDate.CombineLatest(DueDate, NeedsOnDesktop,
                    (start, due, nd) => (start <= due || !nd) ? null : Resources.ReviewErrMsgInvalidTimePeriod),
                StartDateTime.CombineLatest(DueDateTime, NeedsFaceToFace,
                    (start, due, nf) => (start <= due || !nf) ? null : Resources.ReviewErrMsgInvalidTimePeriod),
            }.CombineLatest().Select(msgs => msgs.FirstOrDefault(m => m != null)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsValid = validValue.CombineLatest(validPeriod, (v, p) => v ?? p).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            NeedsOutlookIntegration = NeedsFaceToFace.CombineLatest(
                SettingsModel.Default.ObserveProperty(s => s.CreateTicket.NeedsOutlookIntegration),
                (nf, ni) => nf && ni).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        private void resetPeriod()
        {
            StartDate.Value = DateTime.Today;
            DueDate.Value = SettingsModel.Default.Calendar.GetNextWorkingDay(DateTime.Today, 3);
            StartDateTime.Value = SettingsModel.Default.Calendar.GetNextWorkingDay(DateTime.Today, 3).Add(SettingsModel.Default.Schedule.WorkStartTime);
            DueDateTime.Value = StartDateTime.Value.AddHours(1);
        }

        public override void Clear()
        {
            base.Clear();

            resetPeriod();
        }

        public void ApplyTemplate(PeriodModel template)
        {
            NeedsCreateOutlookAppointment.Value = template.NeedsCreateOutlookAppointment;

            if (template.Method != null && Setting.Value.IsEnabled)
            {
                var method = Setting.Value.GetPossibleValues().FirstOrDefault(a => a.Equals(template.Method));
                if (method != null)
                {
                    SelectedValue.Value = method;
                }
            }
        }

        public DateTime GetStart()
        {
            return NeedsOnDesktop.Value ? StartDate.Value : StartDateTime.Value;
        }

        public DateTime GetDue()
        {
            return NeedsOnDesktop.Value ? DueDate.Value : DueDateTime.Value;
        }

        public string CreatePrgForTicket()
        {
            if (!Setting.Value.IsEnabled)
                return "";

            var label = $"{SelectedValue.Value.Label}";
            if (NeedsFaceToFace.Value)
            {
                var duration = $"{StartDateTime.Value.ToString("yyyy/MM/dd HH:mm")} - {DueDateTime.Value.ToString("yyyy/MM/dd HH:mm")}";
                if (NeedsOnDesktop.Value)
                    label += $" ({Resources.ReviewMethodFaceToFace}:{duration})";
                else
                    label += $" ({duration})";
            }

            return createPrgForTicket(label);
        }
    }
}
