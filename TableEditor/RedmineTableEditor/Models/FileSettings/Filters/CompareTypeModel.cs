using LibRedminePower.Extentions;
using RedmineTableEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings.Filters
{
    public class CompareTypeModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public static CompareTypeModel EQUALS = new CompareTypeModel(IssueStatusCompareType.Equals);
        public static CompareTypeModel NOT_EQUALS = new CompareTypeModel(IssueStatusCompareType.NotEquals);
        public static CompareTypeModel OPEN = new CompareTypeModel(IssueStatusCompareType.Open);
        public static CompareTypeModel CLOSED = new CompareTypeModel(IssueStatusCompareType.Closed);
        public static CompareTypeModel NONE = new CompareTypeModel(IssueStatusCompareType.None);
        public static CompareTypeModel CONTAINS = new CompareTypeModel(IssueStatusCompareType.Contains);
        public static CompareTypeModel NOT_CONTAINS = new CompareTypeModel(IssueStatusCompareType.NotContains);
        public static CompareTypeModel STARTS_WITH = new CompareTypeModel(IssueStatusCompareType.StartsWith);
        public static CompareTypeModel ENDS_WITH = new CompareTypeModel(IssueStatusCompareType.EndsWith);
        public static CompareTypeModel RANGE = new CompareTypeModel(IssueStatusCompareType.Range);
        public static CompareTypeModel GRATER_EQUAL = new CompareTypeModel(IssueStatusCompareType.GreaterEqual);
        public static CompareTypeModel LESS_EQUAL = new CompareTypeModel(IssueStatusCompareType.LessEqual);
        public static CompareTypeModel AFTER = new CompareTypeModel(IssueStatusCompareType.After);
        public static CompareTypeModel BEFORE = new CompareTypeModel(IssueStatusCompareType.Before);
        public static CompareTypeModel NEXT_N_DAYS = new CompareTypeModel(IssueStatusCompareType.NextNDays);
        public static CompareTypeModel LAST_N_DAYS = new CompareTypeModel(IssueStatusCompareType.LastNDays);
        public static CompareTypeModel TOMORROW = new CompareTypeModel(IssueStatusCompareType.Tomorrow);
        public static CompareTypeModel TODAY = new CompareTypeModel(IssueStatusCompareType.Today);
        public static CompareTypeModel YESTERDAY = new CompareTypeModel(IssueStatusCompareType.Yesterday);
        public static CompareTypeModel NEXT_WEEK = new CompareTypeModel(IssueStatusCompareType.NextWeek);
        public static CompareTypeModel THIS_WEEK = new CompareTypeModel(IssueStatusCompareType.ThisWeek);
        public static CompareTypeModel LAST_WEEK = new CompareTypeModel(IssueStatusCompareType.LastWeek);
        public static CompareTypeModel LAST_2_WEEKS = new CompareTypeModel(IssueStatusCompareType.Last2Weeks);
        public static CompareTypeModel NEXT_MONTH = new CompareTypeModel(IssueStatusCompareType.NextMonth);
        public static CompareTypeModel THIS_MONTH = new CompareTypeModel(IssueStatusCompareType.ThisMonth);
        public static CompareTypeModel LAST_MONTH = new CompareTypeModel(IssueStatusCompareType.LastMonth);
        public static CompareTypeModel THIS_YEAR = new CompareTypeModel(IssueStatusCompareType.ThisYear);
        public static CompareTypeModel ANY = new CompareTypeModel(IssueStatusCompareType.Any);

        // 本クラスは RadComboBox でマルチ選択を可能にすると言語切り替えが一部機能しない問題に対応するため作成
        // http://133.242.159.37/issues/398
        public string Name => Type.GetDescription();
        public IssueStatusCompareType Type { get; set; }

        public CompareTypeModel()
        {
        }

        private CompareTypeModel(IssueStatusCompareType type)
        {
            Type = type;
        }

        public override bool Equals(object obj)
        {
            return obj is CompareTypeModel model &&
                   Type == model.Type;
        }

        public override int GetHashCode()
        {
            return 2049151605 + Type.GetHashCode();
        }

        public override string ToString() => Name;

        public string GetEqualSign()
        {
            switch (Type)
            {
                case IssueStatusCompareType.Equals:       return "==";
                case IssueStatusCompareType.NotEquals:    return "=!";
                case IssueStatusCompareType.Open:         return "=o";
                case IssueStatusCompareType.Closed:       return "=c";
                case IssueStatusCompareType.None:         return "=!*";
                case IssueStatusCompareType.Contains:     return "=~";
                case IssueStatusCompareType.NotContains:  return "=!~";
                case IssueStatusCompareType.StartsWith:   return "=^";
                case IssueStatusCompareType.EndsWith:     return "=$";
                case IssueStatusCompareType.Range:        return "=><";
                case IssueStatusCompareType.GreaterEqual: return "=>=";
                case IssueStatusCompareType.LessEqual:    return "=<=";
                case IssueStatusCompareType.After:        return "=>=";
                case IssueStatusCompareType.Before:       return "=<=";
                // エンコードしないとうまく機能しなかったため（元の文字列は「=><t+」）
                case IssueStatusCompareType.NextNDays:    return "=%3E%3Ct%2B";
                case IssueStatusCompareType.LastNDays:    return "=><t-";
                case IssueStatusCompareType.Tomorrow:     return "=nd";
                case IssueStatusCompareType.Today:        return "=t";
                case IssueStatusCompareType.Yesterday:    return "=ld";
                case IssueStatusCompareType.NextWeek:     return "=nw";
                case IssueStatusCompareType.ThisWeek:     return "=w";
                case IssueStatusCompareType.LastWeek:     return "=lw";
                case IssueStatusCompareType.Last2Weeks:   return "=l2w";
                case IssueStatusCompareType.NextMonth:    return "=nm";
                case IssueStatusCompareType.ThisMonth:    return "=m";
                case IssueStatusCompareType.LastMonth:    return "=lm";
                case IssueStatusCompareType.ThisYear:     return "=y";
                case IssueStatusCompareType.Any:          return "=*";
                default: throw new NotSupportedException();
            }
        }

        public bool NeedsInput()
        {
            switch (Type)
            {
                case IssueStatusCompareType.Equals:
                case IssueStatusCompareType.NotEquals:
                case IssueStatusCompareType.Contains:
                case IssueStatusCompareType.NotContains:
                case IssueStatusCompareType.StartsWith:
                case IssueStatusCompareType.EndsWith:
                case IssueStatusCompareType.Range:
                case IssueStatusCompareType.GreaterEqual:
                case IssueStatusCompareType.LessEqual:
                case IssueStatusCompareType.After:
                case IssueStatusCompareType.Before:
                case IssueStatusCompareType.NextNDays:
                case IssueStatusCompareType.LastNDays:
                    return true;
                default:
                    return false;
            }
        }
    }
}
