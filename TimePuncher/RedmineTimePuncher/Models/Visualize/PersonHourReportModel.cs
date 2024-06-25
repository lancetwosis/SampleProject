using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize
{
    public class PersonHourReportModel : LibRedminePower.Models.Bases.ModelBase
    {
        public ReportPeriodType Period { get; set; }

        public PersonHourModel<MyAppointment> OnTimes { get; set; }
        public PersonHourModel<MyAppointment> OnTimesScheduled { get; set; }
        public PersonHourModel<MyAppointment> OnTimesNotScheduled { get; set; }

        public PersonHourModel<MyAppointment> OnTimesRemaining { get; set; }
        public PersonHourModel<MyAppointment> OnTimesScheduledRemaining { get; set; }
        public PersonHourModel<MyAppointment> OnTimesNotScheduledRemaining { get; set; }

        public PersonHourModel<MyAppointment> OverTimesScheduled { get; set; }
        public PersonHourModel<MyAppointment> OverTimesScheduledRemaining { get; set; }

        public PersonHourModel<MyTimeEntry> ActualTimes { get; set; }
        public PersonHourModel<MyTimeEntry> ActualTimesOnTime { get; set; }
        public PersonHourModel<MyTimeEntry> ActualTimesOverTime { get; set; }
        public PersonHourModel<MyTimeEntry> NotWorkedFlexibleTimes { get; set; }

        public PersonHourReportModel(ReportPeriodType period)
        {
            Period = period;

            OnTimes = new PersonHourModel<MyAppointment>();
            OnTimesScheduled = new PersonHourModel<MyAppointment>();
            OnTimesNotScheduled = new PersonHourModel<MyAppointment>();

            OnTimesRemaining = new PersonHourModel<MyAppointment>(period.IsCurrent());
            OnTimesScheduledRemaining = new PersonHourModel<MyAppointment>(period.IsCurrent());
            OnTimesNotScheduledRemaining = new PersonHourModel<MyAppointment>(period.IsCurrent());

            OverTimesScheduled = new PersonHourModel<MyAppointment>();
            OverTimesScheduledRemaining = new PersonHourModel<MyAppointment>(period.IsCurrent());

            ActualTimes = new PersonHourModel<MyTimeEntry>(period.HasActualTimes());
            ActualTimesOnTime = new PersonHourModel<MyTimeEntry>(period.HasActualTimes());
            ActualTimesOverTime = new PersonHourModel<MyTimeEntry>(period.HasActualTimes());
            NotWorkedFlexibleTimes = new PersonHourModel<MyTimeEntry>(period.HasActualTimes());
        }

        public void UpdateScheduledTimes(SettingsModel settings, OutlookManager outlook)
        {
            var start = settings.Schedule.GetStart(Period);
            var end = settings.Schedule.GetEnd(Period);
            var allAppos = outlook.GetAppointments(start, end);

            // 日付のレベルで稼働日に設定されている日付の「定時内」の時間をまとめて定時内総時間にする
            var onTimes = settings.Schedule.GetDays(start, end).Where(d => settings.Calendar.IsWorkingDay(d))
                .SelectMany(d => settings.Schedule.GetOnTimes(d))
                .ToList().Distinct();

            // Outlook から休みの予定を取得し、その分を onTimes から差し引く
            var offTimes = allAppos.Where(a => !settings.Calendar.IsOnTimeAppointment(a)).OrderBy(a => a.Start).ToList().Distinct();
            onTimes = onTimes.Subtract(offTimes);

            OnTimes.Times = onTimes;

            var appos = allAppos.Where(a => settings.Calendar.IsOnTimeAppointment(a)).ToList();

            var onTimeTerms = settings.Schedule.SpecialTerms.Where(t => t.OnTime).ToList();

            var scheduled = new List<MyAppointment>();
            var scheduledOverTime = new List<MyAppointment>();
            foreach (var a in appos.Distinct())
            {
                // 「入力単位（分）」ごとに「定時内」「定時外」を判定する
                for (var s = a.Start; s < a.End; s = s.AddMinutes(settings.Schedule.TickLength))
                {
                    var e = s.AddMinutes(settings.Schedule.TickLength);
                    if (a.End < e)
                        e = a.End;

                    // 日付のレベルで稼働日に設定されており、「定時内」の時間に含まれていたら scheduled に追加
                    if (settings.Calendar.IsWorkingDay(s) &&
                        onTimeTerms.Any(t => t.Start <= s.TimeOfDay && e.TimeOfDay <= t.End))
                        scheduled.Add(new MyAppointment(s, e));
                    else
                        scheduledOverTime.Add(new MyAppointment(s, e));
                }
            }
            scheduled = scheduled.Distinct();
            scheduledOverTime = scheduledOverTime.Distinct();

            // 休みの予定を scheduled から差し引き、scheduledOverTime に追加する
            var subtracted = scheduled.Subtract(offTimes);
            var diff = scheduled.Subtract(subtracted);
            scheduled = subtracted;
            scheduledOverTime.AddRange(diff);
            scheduledOverTime = scheduledOverTime.Distinct();

            OnTimesScheduled.Times = subtracted;
            OnTimesNotScheduled.Times = OnTimes.Times.Subtract(OnTimesScheduled.Times);
            OverTimesScheduled.Times = scheduledOverTime;

            if (OnTimesRemaining.IsEnabled)
            {
                var now = DateTime.Now;
                OnTimesRemaining.Times = OnTimes.Times.Take(now, end);
                OnTimesScheduledRemaining.Times = OnTimesScheduled.Times.Take(now, end);
                OnTimesNotScheduledRemaining.Times = OnTimesRemaining.Times.Subtract(OnTimesScheduledRemaining.Times);
                OverTimesScheduledRemaining.Times = OverTimesScheduled.Times.Take(now, end);
            }
        }

        public async Task UpdateActualTimesAsync(SettingsModel settings, RedmineManager redmine, OutlookManager outlook)
        {
            if (!Period.HasActualTimes())
                return;

            var start = settings.Schedule.GetStart(Period);
            var end = Period.IsCurrent() ? DateTime.Now : settings.Schedule.GetEnd(Period);

            List<MyTimeEntry> entries = null;
            try
            {
                entries = await Task.Run(() => redmine.GetMyTimeEntries(start, end, out var errs));
            }
            catch
            {
                ActualTimes.Times = new List<MyTimeEntry>();
                ActualTimesOnTime.Times = new List<MyTimeEntry>();
                ActualTimesOverTime.Times = new List<MyTimeEntry>();
                NotWorkedFlexibleTimes.Times = new List<MyTimeEntry>();
                return;
            }

            // 「作業時間として計上」が true の作業分類が設定された TimeEntry をまとめて総実績時間とする
            ActualTimes.Times = entries.Where(e => settings.Category.IsWorkingTime(e)).ToList().Distinct();

            var onTimeTerms = settings.Schedule.SpecialTerms.Where(t => t.OnTime).ToList();
            var onTimes = new List<MyTimeEntry>();
            var overTimes = new List<MyTimeEntry>();
            foreach (var a in ActualTimes.Times)
            {
                // 「入力単位（分）」ごとに「定時内」「定時外」を判定する
                for (var s = a.Start; s < a.End; s = s.AddMinutes(settings.Schedule.TickLength))
                {
                    var e = s.AddMinutes(settings.Schedule.TickLength);
                    if (a.End < e)
                        e = a.End;

                    // 日付のレベルで稼働日に設定されており、「定時内」の時間に含まれていたら onTimes に追加
                    if (settings.Calendar.IsWorkingDay(s) &&
                        onTimeTerms.Any(t => t.Start <= s.TimeOfDay && e.TimeOfDay <= t.End))
                        onTimes.Add(new MyTimeEntry(s, e));
                    else
                        overTimes.Add(new MyTimeEntry(s, e));
                }
            }
            onTimes = onTimes.Distinct();
            overTimes = overTimes.Distinct();

            // Outlook から休みの予定を取得し、その分を onTimes から差し引き、overTimes に追加する
            var offTimes = outlook.GetAppointments(start, end)
                .Where(a => !settings.Calendar.IsOnTimeAppointment(a))
                .Select(a => new MyTimeEntry(a.Start, a.End)).OrderBy(a => a.Start).ToList().Distinct();

            var subtracted = onTimes.Subtract(offTimes).Distinct();
            var diff = onTimes.Subtract(subtracted);
            onTimes = subtracted.Distinct();
            overTimes.AddRange(diff);
            overTimes = overTimes.Distinct();

            ActualTimesOnTime.Times = onTimes;
            ActualTimesOverTime.Times = overTimes;

            if (settings.Schedule.UseFlexTime)
            {
                var flexibleTimes = settings.Schedule.GetDays(start, end).Where(d => settings.Calendar.IsWorkingDay(d))
                    .SelectMany(d => settings.Schedule.GetFlexibleTimes(d)).ToList().Distinct();
                // 休みの予定に含まれていた場合、flexibleTimes として扱わない
                flexibleTimes = flexibleTimes.Subtract(offTimes);

                NotWorkedFlexibleTimes.IsEnabled = true;
                NotWorkedFlexibleTimes.Times = flexibleTimes.Subtract(onTimes);
            }
            else
            {
                NotWorkedFlexibleTimes.IsEnabled = false;
                NotWorkedFlexibleTimes.Times = new List<MyTimeEntry>();
            }
        }
    }
}
