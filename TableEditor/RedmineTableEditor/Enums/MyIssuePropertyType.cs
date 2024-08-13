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
    public enum MyIssuePropertyType
    {
        [LocalizedDescription(nameof(Resources.enumMyIssuePropertyTypeMySpentHours), typeof(Resources))]
        MySpentHours,
        [LocalizedDescription(nameof(Resources.enumMyIssuePropertyTypeDiffEstimatedSpent), typeof(Resources))]
        DiffEstimatedSpent,
        [LocalizedDescription(nameof(Resources.enumMyIssuePropertyTypeReplyCount), typeof(Resources))]
        ReplyCount,
    }

    public static class MyIssuePropertyTypeEx
    {
        public static FieldFormat ToFieldFormat(this MyIssuePropertyType type)
        {
            switch (type)
            {
                case MyIssuePropertyType.MySpentHours:
                case MyIssuePropertyType.DiffEstimatedSpent: return FieldFormat.@float;
                case MyIssuePropertyType.ReplyCount:         return FieldFormat.@int;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
