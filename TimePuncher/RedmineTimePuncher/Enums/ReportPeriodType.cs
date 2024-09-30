using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ReportPeriodType
    {
        [LocalizedDescription(nameof(Resources.enumReportPeriodThisWeek), typeof(Resources))]
        ThisWeek,
        [LocalizedDescription(nameof(Resources.enumReportPeriodThisMonth), typeof(Resources))]
        ThisMonth,
        [LocalizedDescription(nameof(Resources.enumReportPeriodLastWeek), typeof(Resources))]
        LastWeek,
        [LocalizedDescription(nameof(Resources.enumReportPeriodLastMonth), typeof(Resources))]
        LastMonth,
        [LocalizedDescription(nameof(Resources.enumReportPeriodNextWeek), typeof(Resources))]
        NextWeek,
        [LocalizedDescription(nameof(Resources.enumReportPeriodNextMonth), typeof(Resources))]
        NextMonth
    }

    public static class ReportPeriodTypeEx
    {
        public static bool IsCurrent(this ReportPeriodType type)
        {
            switch (type)
            {
                case ReportPeriodType.ThisWeek:
                case ReportPeriodType.ThisMonth:
                    return true;
                case ReportPeriodType.LastWeek:
                case ReportPeriodType.LastMonth:
                case ReportPeriodType.NextWeek:
                case ReportPeriodType.NextMonth:
                    return false;
                default:
                    throw new NotSupportedException($"type が {type} はサポート対象外です。");
            }
        }

        public static bool HasActualTimes(this ReportPeriodType type)
        {
            switch (type)
            {
                case ReportPeriodType.ThisWeek:
                case ReportPeriodType.ThisMonth:
                case ReportPeriodType.LastWeek:
                case ReportPeriodType.LastMonth:
                    return true;
                case ReportPeriodType.NextWeek:
                case ReportPeriodType.NextMonth:
                    return false;
                default:
                    throw new NotSupportedException($"type が {type} はサポート対象外です。");
            }
        }
    }
}
