using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedmineTableEditor.Properties;
using RedmineTableEditor.Models.TicketFields.Standard;
using System.Collections;
using RedmineTableEditor.Models;

namespace RedmineTableEditor.Enums
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
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeMySpentHours), typeof(Resources))]
        MySpentHours,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeDiffEstimatedSpent), typeof(Resources))]
        DiffEstimatedSpent,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeTotalSpentHours), typeof(Resources))]
        TotalSpentHours,
        [LocalizedDescription(nameof(Resources.enumIssuePropertyTypeTotalEstimatedHours), typeof(Resources))]
        TotalEstimatedHours,
    }

    public static class IssuePropertyTypeEx
    {
        /// <summary>
        /// 親チケットのカラムの選択肢として有効かどうかを返す。
        /// （「合計作業時間」と「合計予想工数」は親チケットでは無効）
        /// </summary>
        public static bool IsTargetParrent(this IssuePropertyType type)
        {
            switch (type)
            {
                case IssuePropertyType.Id:
                case IssuePropertyType.Subject:
                case IssuePropertyType.Tracker:
                case IssuePropertyType.Status:
                case IssuePropertyType.AssignedTo:
                case IssuePropertyType.FixedVersion:
                case IssuePropertyType.Priority:
                case IssuePropertyType.Category:
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours:
                    return true;
                case IssuePropertyType.MySpentHours:
                case IssuePropertyType.DiffEstimatedSpent:
                default:
                    return false;
            }
        }

        /// <summary>
        /// 取得するために GetObject<Issue> を実行する必要があるかを返す。
        /// 戻り値が true になるプロパティは GetObjects<Issue> では取得できず null になる。
       /// </summary>
        public static bool IsDetail(this IssuePropertyType type)
        {
            switch (type)
            {
                case IssuePropertyType.SpentHours:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours:
                    return true;
                case IssuePropertyType.Id:
                case IssuePropertyType.Subject:
                case IssuePropertyType.Tracker:
                case IssuePropertyType.Status:
                case IssuePropertyType.AssignedTo:
                case IssuePropertyType.FixedVersion:
                case IssuePropertyType.Priority:
                case IssuePropertyType.Category:
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.MySpentHours:
                case IssuePropertyType.DiffEstimatedSpent:
                default:
                    return false;
            }
        }

        public static FieldFormat ToFieldFormat(this IssuePropertyType type)
        {
            switch (type)
            {
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:             return FieldFormat.date;
                case IssuePropertyType.AssignedTo:          return FieldFormat.user;
                case IssuePropertyType.FixedVersion:        return FieldFormat.version;
                case IssuePropertyType.Subject:             return FieldFormat.@string;
                case IssuePropertyType.Id:                  return FieldFormat.@int;
                case IssuePropertyType.Tracker:
                case IssuePropertyType.Status:              return FieldFormat.list;
                case IssuePropertyType.Priority:
                case IssuePropertyType.Category:            return FieldFormat.enumeration;
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
                case IssuePropertyType.MySpentHours:
                case IssuePropertyType.DiffEstimatedSpent:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours: return FieldFormat.@float;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public static IEnumerable GetPropertyItemSource(this IssuePropertyType prop, RedmineManager r)
        {
            switch (prop)
            {
                case IssuePropertyType.Tracker:      return r.Trackers;
                case IssuePropertyType.Status:       return r.Statuses;
                case IssuePropertyType.AssignedTo:   return r.Users;
                case IssuePropertyType.FixedVersion: return r.Versions;
                case IssuePropertyType.Category:     return r.Categories;
                case IssuePropertyType.Priority:     return r.Priorities;
                case IssuePropertyType.Id:
                case IssuePropertyType.Subject:
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
                case IssuePropertyType.MySpentHours:
                case IssuePropertyType.DiffEstimatedSpent:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours:
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
