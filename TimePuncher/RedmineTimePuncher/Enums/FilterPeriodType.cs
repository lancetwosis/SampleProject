using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    public enum FilterPeriodType
    {
        LastWeek,
        LastMonth,
        SpecifyPeriod,
    }

    public static class FilterPeriodTypeEx
    {
        public static bool IsRelative(this FilterPeriodType type)
        {
            switch (type)
            {
                case FilterPeriodType.LastWeek:
                case FilterPeriodType.LastMonth:
                    return true;
                case FilterPeriodType.SpecifyPeriod:
                    return false;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
