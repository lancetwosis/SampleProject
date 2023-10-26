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
                case InputPeriodType.Last3Days:
                    return 1;
                case InputPeriodType.ThisWeek:
                case InputPeriodType.WorkingDays:
                case InputPeriodType.Last7Days:
                    return 7;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
