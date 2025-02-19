using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models.Bases;
using RedmineTableEditor.Models.TicketFields.Custom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RedmineTableEditor.Converters
{
    public class ColumnIsReadOnlyConverter : LibRedminePower.Converters.Bases.ConverterBase<MyIssueBase, bool>
    {
        public override bool Convert(MyIssueBase value, object parameter, CultureInfo culture)
        {
            // チケットに紐づいていなければ編集不可
            if (value == null || value.Issue == null)
                return true;

            // 通常のフィールドなら編集可
            var cf = parameter as CustomField;
            if (cf == null)
                return false;

            var format = cf.ToFieldFormat();
            var prop = typeof(MyIssueBase).GetProperty(format.GetPropertyName()).GetValue(value);
            switch (format)
            {
                // DicCustomFieldXXX に値が存在しなかったら編集不可
                case FieldFormat.@string:
                case FieldFormat.text:
                case FieldFormat.link:
                case FieldFormat.list:
                    return !containsKey<CfString>(cf, prop);
                case FieldFormat.@float:
                    return !containsKey<CfFloat>(cf, prop);
                case FieldFormat.@bool:
                    return !containsKey<CfBool>(cf, prop);
                case FieldFormat.@int:
                case FieldFormat.user:
                case FieldFormat.version:
                case FieldFormat.enumeration:
                    return !containsKey<CfInt>(cf, prop);
                case FieldFormat.date:
                    return !containsKey<CfDate>(cf, prop);
                case FieldFormat.version_multi:
                case FieldFormat.user_multi:
                    return !containsKey<CfInts>(cf, prop);
                case FieldFormat.list_multi:
                case FieldFormat.enumeration_multi:
                    return !containsKey<CfStrings>(cf, prop);
                default:
                    throw new NotSupportedException($"format が {format} は、サポート対象外です。");
            }
        }

        private bool containsKey<T>(CustomField cf, object prop)
        {
            var dic = prop as Dictionary<int, T>;
            return dic.ContainsKey(cf.Id);
        }

        public override MyIssueBase ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
