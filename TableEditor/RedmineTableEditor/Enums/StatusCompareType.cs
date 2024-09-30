using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTableEditor.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RedmineTableEditor.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StatusCompareType
    {
        [LocalizedDescription(nameof(Resources.enumStatusCompareTypeEquals), typeof(Resources))]
        Equals,
        [LocalizedDescription(nameof(Resources.enumStatusCompareTypeNotEquals), typeof(Resources))]
        NotEquals,
    }

    public static class StatusCompareTypeEx
    {
        public static bool IsMatch(this StatusCompareType statusCompare, int statusId, ObservableCollection<int> targetStatusIds)
        {
            switch (statusCompare)
            {
                case StatusCompareType.Equals:
                    return targetStatusIds.Contains(statusId);
                case StatusCompareType.NotEquals:
                    return !targetStatusIds.Contains(statusId);
                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
