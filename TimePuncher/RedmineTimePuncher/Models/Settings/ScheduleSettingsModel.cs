using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RedmineTimePuncher.Models.Settings
{
    public class ScheduleSettingsModel : Bases.SettingsModelBase<ScheduleSettingsModel>
    {
        public TickLengthType TickLength { get; set; } = TickLengthType.TickLength15;
        public TimeSpan DayStartTime { get; set; } = DEFAULT_DAY_START;
        public TimeSpan WorkStartTime { get; set; } = DEFAULT_WORK_START;
        public bool UseFlexTime { get; set; }
        public ObservableCollection<TermModel> SpecialTerms { get; set; } = new ObservableCollection<TermModel>()
        {
            new TermModel(new TimeSpan(09,0,0), new TimeSpan(10,0,0)) {Color = System.Drawing.Color.FromArgb(241, 191, 190) },
            new TermModel(new TimeSpan(10,0,0), new TimeSpan(11,0,0)) {Color = System.Drawing.Color.FromArgb(247, 237, 225) },
            new TermModel(new TimeSpan(11,0,0), new TimeSpan(12,0,0)) {Color = System.Drawing.Color.FromArgb(145, 214, 202) },
            new TermModel(new TimeSpan(12,0,0), new TimeSpan(13,0,0)),
            new TermModel(new TimeSpan(13,0,0), new TimeSpan(14,0,0)) {Color = System.Drawing.Color.FromArgb(250, 226, 160) },
            new TermModel(new TimeSpan(14,0,0), new TimeSpan(15,0,0)) {Color = System.Drawing.Color.FromArgb(146, 184, 225) },
            new TermModel(new TimeSpan(15,0,0), new TimeSpan(16,0,0)) {Color = System.Drawing.Color.FromArgb(127, 118, 162) },
            new TermModel(new TimeSpan(16,0,0), new TimeSpan(17,0,0)) {Color = System.Drawing.Color.FromArgb(241, 191, 190) },
            new TermModel(new TimeSpan(17,0,0), new TimeSpan(18,0,0)) {Color = System.Drawing.Color.FromArgb(247, 237, 225) },
        };

        public static int DEFAUT_TICK_LENGTH = 15;
        public static TimeSpan DEFAULT_DAY_START = new TimeSpan(5, 0, 0);
        public static TimeSpan DEFAULT_WORK_START = new TimeSpan(9, 0, 0);

        public ScheduleSettingsModel()
        {
        }

        public DateTime GetToday()
        {
            return DateTime.Today.AddHours(DayStartTime.Hours).AddMinutes(DayStartTime.Minutes);
        }

        public DateTime GetFirstDayOfWeek()
        {
            var today = GetToday();

            // 今日が日曜日で、かつ、現在時刻が DayStartTime 以下である場合、先週に属すると判定する
            if (today.DayOfWeek == DayOfWeek.Sunday)
                return DateTime.Now < today ? today.AddDays(-7) : today;

            while (true)
            {
                today = today.AddDays(-1);

                if (today.DayOfWeek == DayOfWeek.Sunday)
                    return today;
            }
        }

        /// <summary>
        /// 週の終わりの時刻（日曜日の DayStartTime の時刻）を返す。
        /// </summary>
        public DateTime GetLastDayOfWeek()
        {
            var today = GetToday();

            // 今日が日曜日で、かつ、現在時刻が DayStartTime 以下である場合、先週に属すると判定する
            if (today.DayOfWeek == DayOfWeek.Sunday)
                return DateTime.Now < today ? today : today.AddDays(7);

            while (true)
            {
                today = today.AddDays(1);

                if (today.DayOfWeek == DayOfWeek.Sunday)
                    return today;
            }
        }

        public DateTime GetFirstDayOfMonth()
        {
            var today = GetToday();
            var firstDay = new DateTime(today.Year, today.Month, 1, today.Hour, today.Minute, today.Second);

            // 今日が１日で、かつ、現在時刻が DayStartTime 以下である場合、先月に属すると判定する
            if (today.Day == 1 && DateTime.Now < today)
                return firstDay.AddMonths(-1);
            else
                return firstDay;
        }

        /// <summary>
        /// 月の終わりの時刻（翌月の１日の DayStartTime の時刻）を返す。
        /// </summary>
        public DateTime GetLastDayOfMonth()
        {
            var today = GetToday();
            var lastDay = new DateTime(today.Year, today.AddMonths(1).Month, 1, today.Hour, today.Minute, today.Second);

            // 今日が１日で、かつ、現在時刻が DayStartTime 以下である場合、先月に属すると判定する
            if (today.Day == 1 && DateTime.Now < today)
                return lastDay.AddMonths(-1);
            else
                return lastDay;
        }

        /// <summary>
        /// 期間の始まりの日の DayStartTime の時刻を返す
        /// </summary>
        public DateTime GetStart(ReportPeriodType period)
        {
            switch (period)
            {
                case ReportPeriodType.ThisWeek:
                    return GetFirstDayOfWeek();
                case ReportPeriodType.ThisMonth:
                    return GetFirstDayOfMonth();
                case ReportPeriodType.LastWeek:
                    return GetFirstDayOfWeek().AddDays(-7);
                case ReportPeriodType.LastMonth:
                    return GetFirstDayOfMonth().AddMonths(-1);
                case ReportPeriodType.NextWeek:
                    return GetFirstDayOfWeek().AddDays(7);
                case ReportPeriodType.NextMonth:
                    return GetFirstDayOfMonth().AddMonths(1);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 期間の終わりの日の翌日の DayStartTime の時刻を返す。日付を使った処理には注意すること。
        /// </summary>
        public DateTime GetEnd(ReportPeriodType period)
        {
            switch (period)
            {
                case ReportPeriodType.ThisWeek:
                    return GetLastDayOfWeek();
                case ReportPeriodType.ThisMonth:
                    return GetLastDayOfMonth();
                case ReportPeriodType.LastWeek:
                    return GetLastDayOfWeek().AddDays(-7);
                case ReportPeriodType.LastMonth:
                    return GetLastDayOfMonth().AddMonths(-1);
                case ReportPeriodType.NextWeek:
                    return GetLastDayOfWeek().AddDays(7);
                case ReportPeriodType.NextMonth:
                    return GetLastDayOfMonth().AddMonths(1);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// それぞれの時刻が DayStartTime に達していなかったら、前日に属すると判定し、間に含まれる日付のリストを返す
        /// </summary>
        public List<DateTime> GetDays(DateTime start, DateTime end)
        {
            var startDay = start.TimeOfDay < DayStartTime ? start.AddDays(-1) : start;
            var endDay = end.TimeOfDay <= DayStartTime ? end.AddDays(-1) : end;
            return startDay.GetDays(endDay).ToList();
        }

        public List<MyAppointment> GetOnTimes(DateTime date)
        {
            return SpecialTerms.Where(t => t.OnTime).Select(t => new MyAppointment(
                new DateTime(date.Year, date.Month, date.Day, t.Start.Hours, t.Start.Minutes, 0),
                new DateTime(date.Year, date.Month, date.Day, t.End.Hours, t.End.Minutes, 0))).ToList();
        }

        public List<MyTimeEntry> GetFlexibleTimes(DateTime date)
        {
            if (!UseFlexTime)
                return new List<MyTimeEntry>();

            return SpecialTerms.Where(t => t.OnTime && !t.IsCoreTime).Select(t => new MyTimeEntry(
                new DateTime(date.Year, date.Month, date.Day, t.Start.Hours, t.Start.Minutes, 0),
                new DateTime(date.Year, date.Month, date.Day, t.End.Hours, t.End.Minutes, 0))).ToList();
        }

        public string GetPeriodString(ReportPeriodType period)
        {
            var start = GetStart(period);
            var end = GetEnd(period);

            // end には次の日の DayStartTime が設定されているため期間表示では前日の日付を表示する
            return $"{start.ToString("MM/dd")} - {end.AddDays(-1).ToString("MM/dd")}";
        }

        private static SolidColorBrush TRANSPARENT = new SolidColorBrush(Colors.Transparent);
        public System.Windows.Media.Brush GetTickBackground(DateTime targetTime, double? sliderValue = null)
        {
            if (sliderValue.HasValue && TickLength.NeedsTransparent(sliderValue.Value))
                return TRANSPARENT;

            var target = targetTime.TimeOfDay;
            var term = SpecialTerms.FirstOrDefault(a => a.Start <= target && target < a.End);

            // sliderValue は MinorTick の場合のみ、バインドされているためそれにより判定可能
            return term?.GetTickBackground(!sliderValue.HasValue);
        }

        public Visibility GetMinorTickVisibility(double sliderValue)
        {
            if (TickLength.NeedsCollapsed(sliderValue))
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public List<MyTimeEntry> DevideIfNeeded(MyTimeEntry entry)
        {
            var onTimeTerms = SpecialTerms.Where(t => t.OnTime).ToList();
            var results = new List<MyTimeEntry>();

            for (var s = entry.Start; s < entry.End; s = s.AddMinutes(TickLength))
            {
                var e = s.AddMinutes(TickLength);
                if (entry.End < e)
                    e = entry.End;

                if (onTimeTerms.Any(t => t.Start <= s.TimeOfDay && e.TimeOfDay <= t.End))
                {
                    if (results.Count == 0 || results.Last().Type != TimeEntryType.OnTime)
                    {
                        var clone = entry.Clone();
                        clone.Type = TimeEntryType.OnTime;
                        clone.Start = s;
                        clone.End = e;
                        results.Add(clone);
                    }
                    else
                    {
                        var last = results.Last();
                        last.End = e;
                    }
                }
                else
                {
                    if (results.Count == 0 || results.Last().Type != TimeEntryType.OverTime)
                    {
                        var clone = entry.Clone();
                        clone.Type = TimeEntryType.OverTime;
                        clone.Start = s;
                        clone.End = e;
                        results.Add(clone);
                    }
                    else
                    {
                        var last = results.Last();
                        last.End = e;
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// target が date に含まれる予定かどうかを返す
        /// </summary>
        public bool Contains(DateTime date, MyAppointment target)
        {
            var start = date.GetDateOnly().Add(DayStartTime);
            var end = date.GetDateOnly().Add(DayStartTime).AddDays(1);
            return start <= target.Start && target.End <= end;
        }
    }
}
