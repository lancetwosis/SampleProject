using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.Input.Report
{
    public class PersonHourReportViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string Title { get; set; }

        public TimesAreaViewModel<MyAppointment> OnTimes { get; set; }
        public TimesAreaViewModel<MyAppointment> OnTimesRemaining { get; set; }
        public TimesAreaViewModel<MyAppointment> OverTimeAppointments { get; set; }
        public TimesAreaViewModel<MyTimeEntry> ActualTimes { get; set; }

        public ReactiveCommand CopyCommand { get; set; }

        private InputViewModel parent;
        private PersonHourReportSettingModel setting;
        private ReactivePropertySlim<PersonHourReportModel> model;

        public PersonHourReportViewModel(InputViewModel parent, PersonHourReportSettingModel setting)
        {
            this.parent = parent;
            this.setting = setting;
            this.model = new ReactivePropertySlim<PersonHourReportModel>(new PersonHourReportModel(setting.Period)).AddTo(disposables);

            var settingsChanged = parent.Parent.ObserveProperty(p => p.Settings.Schedule).CombineLatest(
                parent.Parent.ObserveProperty(p => p.Settings.Calendar),
                parent.Parent.ObserveProperty(p => p.Settings.Category), (s1, s2, s3) => (s1, s2, s3));

            model.CombineLatest(settingsChanged, (m, s) => (m, s)).SubscribeWithErr(async p =>
            {
                updateTitle();
                await updateScheduledTimesAsync(p.m);
            }).AddTo(disposables);

            model.CombineLatest(parent.Parent.Redmine, settingsChanged, (m, r, s) => (m, r, s)).SubscribeWithErr(async p =>
            {
                updateTitle();
                await updateActualTimesAsync(p.m, p.r);
            }).AddTo(disposables);

            CopyCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                var lines = new[] { OnTimes, OnTimesRemaining, OverTimeAppointments }.Where(a => a.IsVisible).Select(a => a.TsvLines).ToList();
                if (ActualTimes.IsVisible)
                {
                    lines.Add(ActualTimes.TsvLines);
                }

                lines.Insert(0, $"{Title}\t{Properties.Resources.ReportHours}\t%");

                Clipboard.SetText(string.Join(Environment.NewLine, lines));
            }).AddTo(disposables);
        }

        private void updateTitle()
        {
            Title = $"{setting.Period.GetDescription()} ( {parent.Parent.Settings.Schedule.GetPeriodString(setting.Period)} )";
        }

        private CompositeDisposable myDispo1 = null;
        private CancellationTokenSource cts1 = null;
        private async Task updateScheduledTimesAsync(PersonHourReportModel m)
        {
            cts1?.Cancel();
            cts1 = new CancellationTokenSource();
            await Task.Run(() =>
            {
                if (m == null)
                    return;

                m.UpdateScheduledTimes(parent.Parent.Settings, parent.Parent.Outlook);

                myDispo1?.Dispose();
                myDispo1 = new CompositeDisposable().AddTo(disposables);

                OnTimes = new TimesAreaViewModel<MyAppointment>(setting.IsVisible(PersonHourReportContentType.OnTimes),
                    Properties.Resources.enumReportColumnOnTimes, m.OnTimes,
                    Properties.Resources.ReportNotScheduled, m.OnTimesNotScheduled,
                    Properties.Resources.ReportScheduled, m.OnTimesScheduled).AddTo(myDispo1);
                OnTimesRemaining = new TimesAreaViewModel<MyAppointment>(setting.IsVisible(PersonHourReportContentType.OnTimesRemaining),
                    Properties.Resources.enumReportColumnOnTimesRemaining, m.OnTimesRemaining,
                    Properties.Resources.ReportNotScheduled, m.OnTimesNotScheduledRemaining,
                    Properties.Resources.ReportScheduled, m.OnTimesScheduledRemaining).AddTo(myDispo1);
                OverTimeAppointments = new TimesAreaViewModel<MyAppointment>(setting.IsVisible(PersonHourReportContentType.OverTimeAppointment),
                    Properties.Resources.enumReportColumnOverTimeAppointment,
                    Properties.Resources.ReportTotalTime, m.OverTimesScheduled,
                    Properties.Resources.ReportRemainingTime, m.OverTimesScheduledRemaining).AddTo(myDispo1);
            }, cts1.Token);
        }

        private CompositeDisposable myDispo2 = null;
        private CancellationTokenSource cts2 = null;
        private async Task updateActualTimesAsync(PersonHourReportModel m, RedmineManager r)
        {
            cts2?.Cancel();
            cts2 = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                if (m == null || r == null)
                    return;

                await m.UpdateActualTimesAsync(parent.Parent.Settings, r, parent.Parent.Outlook);

                myDispo2?.Dispose();
                myDispo2 = new CompositeDisposable().AddTo(disposables);

                ActualTimes = new TimesAreaViewModel<MyTimeEntry>(setting.IsVisible(PersonHourReportContentType.ActualTimes),
                    Properties.Resources.enumReportColumnActualTimes, m.ActualTimes,
                    Properties.Resources.ReportOnTime, m.ActualTimesOnTime,
                    Properties.Resources.ReportOverTime, m.ActualTimesOverTime, m.NotWorkedFlexibleTimes).AddTo(myDispo2);
            }, cts2.Token);
        }

        public async Task UpdateAsync()
        {
            updateTitle();
            var t1 = updateScheduledTimesAsync(model.Value);
            var t2 = updateActualTimesAsync(model.Value, parent.Parent.Redmine.Value);
            await Task.WhenAll(t1, t2);
        }
    }
}
