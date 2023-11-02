using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum InputPeriodType
    {
        // RadScheduleView.ViewDefinitions に影響するので定義順に注意。
        [LocalizedDescription(nameof(Resources.enumInputPeriodType1Day), typeof(Resources))]
        OneDay,
        [LocalizedDescription(nameof(Resources.enumInputPeriodTypeThisWeek), typeof(Resources))]
        ThisWeek,
        [LocalizedDescription(nameof(Resources.enumInputPeriodTypeWorkingDays), typeof(Resources))]
        WorkingDays,
        [LocalizedDescription(nameof(Resources.enumInputPeriodType3Days), typeof(Resources))]
        Last3Days,
        [LocalizedDescription(nameof(Resources.enumInputPeriodType7Days), typeof(Resources))]
        Last7Days,
    }

    public static class InputPeriodTypeEx
    {
        /// <summary>
        /// 開始日の 00:00:00 を返す
        /// </summary>
        public static DateTime GetStartDate(this InputPeriodType type, DateTime currentDate, CalendarSettingsModel calendar)
        {
            switch (type)
            {
                case InputPeriodType.OneDay:
                    return currentDate;
                case InputPeriodType.Last3Days:
                    return currentDate.AddDays(-2);
                case InputPeriodType.Last7Days:
                    return currentDate.AddDays(-6);
                case InputPeriodType.ThisWeek:
                    return currentDate.GetFirstDayOfWeek();
                case InputPeriodType.WorkingDays:
                    return calendar.GetFirstWorkingDayOfWeek(currentDate);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 終了日の次の日の 00:00:00 を返す。例えば、週の最後が土曜日だった場合、日曜日の日付の 00:00:00 を返す。
        /// </summary>
        public static DateTime GetEndDate(this InputPeriodType type, DateTime currentDate, CalendarSettingsModel calendar)
        {
            switch (type)
            {
                case InputPeriodType.OneDay:
                case InputPeriodType.Last3Days:
                case InputPeriodType.Last7Days:
                    return currentDate.AddDays(1);
                case InputPeriodType.ThisWeek:
                    return currentDate.GetLastDayOfWeek().AddDays(1);
                case InputPeriodType.WorkingDays:
                    return calendar.GetLastWorkingDayOfWeek(currentDate).AddDays(1);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static int GetIntervalDays(this InputPeriodType type)
        {
            switch (type)
            {
                case InputPeriodType.OneDay:
                    return 1;
                case InputPeriodType.Last3Days:
                    return 3;
                case InputPeriodType.ThisWeek:
                case InputPeriodType.WorkingDays:
                case InputPeriodType.Last7Days:
                    return 7;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// currentDate と type から表示範囲を割り出し、そこに targetDate が含まれるかどうかを返す。
        /// </summary>
        public static bool Contains(this InputPeriodType type, DateTime currentDate, DateTime targetDate, CalendarSettingsModel calendar)
        {
            switch (type)
            {
                case InputPeriodType.OneDay:
                case InputPeriodType.Last3Days:
                case InputPeriodType.Last7Days:
                    {
                        var start = type.GetStartDate(currentDate, calendar);
                        var end = type.GetEndDate(currentDate, calendar);
                        return start <= targetDate && targetDate < end;
                    }
                case InputPeriodType.ThisWeek:
                case InputPeriodType.WorkingDays:
                    {
                        var start = currentDate.GetFirstDayOfWeek();
                        var end = currentDate.GetLastDayOfWeek().AddDays(1);
                        return start <= targetDate && targetDate < end;
                    }
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
