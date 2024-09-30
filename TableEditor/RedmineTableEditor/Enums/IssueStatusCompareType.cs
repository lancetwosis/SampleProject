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
    public enum IssueStatusCompareType
    {
        [LocalizedDescription(nameof(Resources.enumStatusCompareTypeEquals), typeof(Resources))]
        Equals,
        [LocalizedDescription(nameof(Resources.enumStatusCompareTypeNotEquals), typeof(Resources))]
        NotEquals,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeOpen), typeof(Resources))]
        Open,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeClosed), typeof(Resources))]
        Closed,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeNone), typeof(Resources))]
        None,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeContains), typeof(Resources))]
        Contains,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDoesnotContain), typeof(Resources))]
        NotContains,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeStartsWith), typeof(Resources))]
        StartsWith,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeEndsWith), typeof(Resources))]
        EndsWith,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateRange), typeof(Resources))]
        Range,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeNumGreaterEquals), typeof(Resources))]
        GreaterEqual,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeNumLessEquals), typeof(Resources))]
        LessEqual,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateAfter), typeof(Resources))]
        After,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateBefore), typeof(Resources))]
        Before,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateNextNDays), typeof(Resources))]
        NextNDays,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateLastNDays), typeof(Resources))]
        LastNDays,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateTomorrow), typeof(Resources))]
        Tomorrow,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateToday), typeof(Resources))]
        Today,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateYesterday), typeof(Resources))]
        Yesterday,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateNextWeek), typeof(Resources))]
        NextWeek,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateThisWeek), typeof(Resources))]
        ThisWeek,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateLastWeek), typeof(Resources))]
        LastWeek,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateLast2Weeks), typeof(Resources))]
        Last2Weeks,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateNextMonth), typeof(Resources))]
        NextMonth,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateThisMonth), typeof(Resources))]
        ThisMonth,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateLastMonth), typeof(Resources))]
        LastMonth,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeDateThisYear), typeof(Resources))]
        ThisYear,
        [LocalizedDescription(nameof(Resources.enumIssueStatusCompareTypeAny), typeof(Resources))]
        Any,
    }
}
