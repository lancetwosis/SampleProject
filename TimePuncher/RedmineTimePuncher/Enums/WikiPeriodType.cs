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
    public enum WikiPeriodType
    {
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeNone), typeof(Resources))]
        None,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeLastHalfYear), typeof(Resources))]
        LastHalfYear,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeLast1Week), typeof(Resources))]
        Last1Weeks,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeLastNWeeks), typeof(Resources))]
        LastNWeeks,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeLast1Month), typeof(Resources))]
        Last1Month,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeLastNMonth), typeof(Resources))]
        LastNMonth,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeBetween), typeof(Resources))]
        Between,
        [LocalizedDescription(nameof(Resources.enumWikiPeriodTypeAll), typeof(Resources))]
        All,
    }

    public static class WikiPeriodTypeEx
    {
        public static (DateTime, DateTime)? GetPeriod(this WikiPeriodType type, int num, DateTime start, DateTime end)
        {
            switch (type)
            {
                case WikiPeriodType.None:
                    return null;
                case WikiPeriodType.LastHalfYear:
                    return (DateTime.Today.AddMonths(-6), DateTime.Today);
                case WikiPeriodType.Last1Weeks:
                    return (DateTime.Today.AddDays(-7), DateTime.Today);
                case WikiPeriodType.LastNWeeks:
                    return (DateTime.Today.AddDays(-7 * num), DateTime.Today);
                case WikiPeriodType.Last1Month:
                    return (DateTime.Today.AddMonths(-1), DateTime.Today);
                case WikiPeriodType.LastNMonth:
                    return (DateTime.Today.AddMonths(-1 * num), DateTime.Today);
                case WikiPeriodType.Between:
                    return (start, end);
                case WikiPeriodType.All:
                    return (DateTime.MinValue, DateTime.MaxValue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type.ToString());
            }
        }
    }
}
