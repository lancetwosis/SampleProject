using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StringCompareType
    {
        [LocalizedDescription(nameof(Resources.enumStringCompareContains), typeof(Resources))]
        Contains,
        [LocalizedDescription(nameof(Resources.enumStringCompareStartWith), typeof(Resources))]
        StartWith,
        [LocalizedDescription(nameof(Resources.enumStringCompareEndWith), typeof(Resources))]
        EndWith,
        [LocalizedDescription(nameof(Resources.enumStringCompareEquals), typeof(Resources))]
        Equals,
    }

    public static class StringCompareTypeEx
    {
        public static bool IsMatch(this StringCompareType compare, string target1, string target2)
        {
            switch (compare)
            {
                case StringCompareType.Contains:
                    return target1.Contains(target2);
                case StringCompareType.StartWith:
                    return target1.StartsWith(target2);
                case StringCompareType.EndWith:
                    return target1.EndsWith(target2);
                case StringCompareType.Equals:
                    return target1 == target2;
                default:
                    throw new InvalidOperationException();
            }

        }
    }
}
