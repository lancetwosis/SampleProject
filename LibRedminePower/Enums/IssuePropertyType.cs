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
    public enum IssuePropertyType
    {
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeId), typeof(Resources))]
        Id,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeSubject), typeof(Resources))]
        Subject,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeTracker), typeof(Resources))]
        Tracker,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeStatus), typeof(Resources))]
        Status,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeAssignedTo), typeof(Resources))]
        AssignedTo,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeFixedVersion), typeof(Resources))]
        FixedVersion,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypePriority), typeof(Resources))]
        Priority,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeCategory), typeof(Resources))]
        Category,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeStartDate), typeof(Resources))]
        StartDate,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeDueDate), typeof(Resources))]
        DueDate,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeDoneRatio), typeof(Resources))]
        DoneRatio,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeEstimatedHours), typeof(Resources))]
        EstimatedHours,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeSpentHours), typeof(Resources))]
        SpentHours,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeTotalSpentHours), typeof(Resources))]
        TotalSpentHours,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeTotalEstimatedHours), typeof(Resources))]
        TotalEstimatedHours,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeAuthor), typeof(Resources))]
        Author,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeUpdated), typeof(Resources))]
        Updated,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeCreated), typeof(Resources))]
        Created,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeProject), typeof(Resources))]
        Project,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeUpdater), typeof(Resources))]
        Updater,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeLastUpdater), typeof(Resources))]
        LastUpdater,
    }
}
