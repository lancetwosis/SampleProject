using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize.Filters
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PeriodType
    {
        [LocalizedDescription(nameof(Resources.VisualizePeriodLastWeek), typeof(Resources))]
        LastWeek,
        [LocalizedDescription(nameof(Resources.VisualizePeriodLastMonth), typeof(Resources))]
        LastMonth,
        [LocalizedDescription(nameof(Resources.VisualizePeriodSpecify), typeof(Resources))]
        SpecifyPeriod,
    }

    public static class FilterPeriodTypeEx
    {
        public static bool IsRelative(this PeriodType type)
        {
            switch (type)
            {
                case PeriodType.LastWeek:
                case PeriodType.LastMonth:
                    return true;
                case PeriodType.SpecifyPeriod:
                    return false;
                default:
                    throw new NotSupportedException($"type が {type} はサポート対象外です。");
            }
        }
    }
}
