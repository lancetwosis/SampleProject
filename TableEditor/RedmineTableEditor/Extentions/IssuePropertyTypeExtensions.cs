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
