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

namespace RedmineTimePuncher.ViewModels.CreateTicket
{
    public class ReviewMethodViewModel : CustomFieldSettingViewModelBase<ReviewMethodSettingModel, ReviewMethodCustomField, ReviewMethodValue>
    {
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime DueDateTime { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> NeedsOnDesktop { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> NeedsFaceToFace { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> IsValidDuration { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> NeedsOutlookIntegration { get; set; }

        [Obsolete("For Serialize", false)]
        public ReviewMethodViewModel() { }

        public ReviewMethodViewModel(SettingsModel model) : base(Resources.ReviewReviewMethod)
        {
            ResetDuration(model);
        }

        protected CompositeDisposable myDisposables { get; set; }
        public void Setup(SettingsModel settings)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable();

            Model = settings.CreateTicket.ReviewMethod;

            var isEnabled = Model.ObserveProperty(a => a.IsEnabled).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            var needsOnDesktop = Model.ObserveProperty(a => a.Value.NeedsOnDesktop).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            var needsFaceToFace = Model.ObserveProperty(a => a.Value.NeedsFaceToFace).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            // 「期間」の選択は以下の条件で表示する
            // 「レビュー方法の指定」が無効 or 机上レビューが有効 or 机上、対面のどちらも無効
            NeedsOnDesktop = isEnabled.CombineLatest(needsOnDesktop, needsFaceToFace, (i, nd, nf) => !i || nd || (!nd && !nf))
                .Throttle(TimeSpan.FromMilliseconds(100)).ObserveOnUIDispatcher().ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            // 「開催日時」の選択は、「レビュー方法の指定」が有効 and 対面レビューが有効の場合のみ、表示する
            NeedsFaceToFace = isEnabled.CombineLatest(needsFaceToFace, (i, nf) => i && nf)
                .Throttle(TimeSpan.FromMilliseconds(100)).ObserveOnUIDispatcher().ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            IsValidDuration = new[]
            {
                this.ObserveProperty(a => a.StartDate).CombineLatest(this.ObserveProperty(a => a.DueDate), NeedsOnDesktop,
                    (s, d, nd) => (s <= d || !nd) ? null : Resources.ReviewErrMsgInvalidTimePeriod),
                this.ObserveProperty(a => a.StartDateTime).CombineLatest(this.ObserveProperty(a => a.DueDateTime), NeedsFaceToFace,
                    (s, d, nf) => (s <= d || !nf) ? null : Resources.ReviewErrMsgInvalidTimePeriod),
            }.CombineLatest().Select(msgs => msgs.FirstOrDefault(m => m != null)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            NeedsOutlookIntegration = NeedsFaceToFace.Select(nf => settings.CreateTicket.NeedsOutlookIntegration && nf)
                .ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
        }

        public void SetPreviousDuration(ReviewMethodViewModel previous)
        {
            if (previous == null)
                return;

            StartDate = previous.StartDate;
            DueDate = previous.DueDate;
            StartDateTime = previous.StartDateTime;
            DueDateTime = previous.DueDateTime;
        }

        public void ResetDuration(SettingsModel settings)
        {
            StartDate = DateTime.Today;
            DueDate = settings.Calendar.GetNextWorkingDay(DateTime.Today, 3);
            StartDateTime = settings.Calendar.GetNextWorkingDay(DateTime.Today, 3).Add(settings.Schedule.WorkStartTime);
            DueDateTime = StartDateTime.AddHours(1);
        }

        public DateTime GetStart()
        {
            return NeedsOnDesktop.Value ? StartDate : StartDateTime;
        }

        public DateTime GetDue()
        {
            return NeedsOnDesktop.Value ? DueDate : DueDateTime;
        }

        public string CreatePrgForTicket()
        {
            if (!Model.IsEnabled)
                return "";

            var label = $"{Model.Value.Label}";
            if (NeedsFaceToFace.Value)
            {
                var duration = $"{StartDateTime.ToString("yyyy/MM/dd HH:mm")} - {DueDateTime.ToString("yyyy/MM/dd HH:mm")}";
                if (NeedsOnDesktop.Value)
                    label += $" ({Resources.ReviewMethodFaceToFace}:{duration})";
                else
                    label += $" ({duration})";
            }

            return createPrgForTicket(label);
        }
    }
}
