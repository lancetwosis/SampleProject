using LibRedminePower.Enums;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Extentions
{
    public static class IssuePropertyTypeExtensions
    {
        /// <summary>
        /// TableEditor のプロパティのカラムとして有効かどうかを返す。
        /// </summary>
        public static bool IsPropertyColumn(this IssuePropertyType type)
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
                case IssuePropertyType.Author:
                case IssuePropertyType.Updated:
                case IssuePropertyType.Created:
                case IssuePropertyType.Project:
                // IssuePropertyType.Updater はフィルターとしては有効だがカラムには表示しない
                case IssuePropertyType.LastUpdater:
                    return true;
                default:
                    return false;
            }
        }

        public static int ToPropertyColumnOrder(this IssuePropertyType type)
        {
            var types = new List<IssuePropertyType>()
            {
                IssuePropertyType.Id,
                IssuePropertyType.Project,
                IssuePropertyType.Status,
                IssuePropertyType.Tracker,
                IssuePropertyType.Priority,
                IssuePropertyType.Author,
                IssuePropertyType.AssignedTo,
                IssuePropertyType.FixedVersion,
                IssuePropertyType.Category,
                IssuePropertyType.Subject,
                IssuePropertyType.DoneRatio,
                IssuePropertyType.Updater,
                IssuePropertyType.LastUpdater,
                IssuePropertyType.Created,
                IssuePropertyType.Updated,
                IssuePropertyType.StartDate,
                IssuePropertyType.DueDate,
                IssuePropertyType.EstimatedHours,
                IssuePropertyType.SpentHours,
                IssuePropertyType.TotalSpentHours,
                IssuePropertyType.TotalEstimatedHours,
            };
            return types.IndexOf(type);
        }

        public static FieldFormat ToFieldFormat(this IssuePropertyType type)
        {
            switch (type)
            {
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.Updated:
                case IssuePropertyType.Created:             return FieldFormat.date;
                case IssuePropertyType.AssignedTo:          return FieldFormat.user;
                case IssuePropertyType.FixedVersion:        return FieldFormat.version;
                case IssuePropertyType.Subject:
                case IssuePropertyType.Project:
                case IssuePropertyType.Author:
                case IssuePropertyType.LastUpdater:         return FieldFormat.@string;
                case IssuePropertyType.Id:                  return FieldFormat.@int;
                case IssuePropertyType.Tracker:
                case IssuePropertyType.Status:              return FieldFormat.list;
                case IssuePropertyType.Priority:
                case IssuePropertyType.Category:            return FieldFormat.enumeration;
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
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
                case IssuePropertyType.Status:       return r.Cache.Statuss;
                case IssuePropertyType.AssignedTo:   return r.Users;
                case IssuePropertyType.FixedVersion: return r.Versions;
                case IssuePropertyType.Category:     return r.Categories;
                case IssuePropertyType.Priority:     return r.Cache.Priorities;
                case IssuePropertyType.Id:
                case IssuePropertyType.Subject:
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours:
                default:
                    throw new NotSupportedException($"prop が {prop} は、サポート対象外です。");
            }
        }
    }
}
