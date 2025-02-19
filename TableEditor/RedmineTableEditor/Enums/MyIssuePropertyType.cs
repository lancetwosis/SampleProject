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
        [LocalizedDescription(nameof(Resources.enumMyIssuePropertyTypeRequiredDays), typeof(Resources))]
        RequiredDays,
        [LocalizedDescription(nameof(Resources.enumMyIssuePropertyTypeDaysUntilCreated), typeof(Resources))]
        DaysUntilCreated,
    }

    public static class MyIssuePropertyTypeEx
    {
        public static FieldFormat ToFieldFormat(this MyIssuePropertyType type)
        {
            switch (type)
            {
                case MyIssuePropertyType.MySpentHours:
                case MyIssuePropertyType.DiffEstimatedSpent:
                case MyIssuePropertyType.RequiredDays:
                case MyIssuePropertyType.DaysUntilCreated:
                    return FieldFormat.@float;
                case MyIssuePropertyType.ReplyCount:
                    return FieldFormat.@int;
                default:
                    throw new NotSupportedException($"type が {type} は、サポート対象外です。");
            }
        }

        public static string GetToolTip(this MyIssuePropertyType type)
        {
            switch (type)
            {
                case MyIssuePropertyType.MySpentHours:
                    return Resources.MySpentHoursToolTip;
                case MyIssuePropertyType.DiffEstimatedSpent:
                    return Resources.DiffEstimatedSpentToolTip;
                case MyIssuePropertyType.ReplyCount:
                    return Resources.ReplyCountToolTip;
                case MyIssuePropertyType.RequiredDays:
                    return Resources.RequiredDaysToolTip;
                case MyIssuePropertyType.DaysUntilCreated:
                    return Resources.DaysUntilCreatedToolTip;
                default:
                    throw new NotSupportedException($"type が {type} は、サポート対象外です。");
            }
        }
    }
}
