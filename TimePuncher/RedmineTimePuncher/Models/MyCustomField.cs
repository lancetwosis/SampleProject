using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.Models
{
    public class MyCustomField<TValue> : IdName where TValue : MyCustomFieldPossibleValue
    {
        public CustomFieldFormat Format { get; set; }
        public List<TValue> PossibleValues { get; set; }

        public MyCustomField()
        {
        }

        public MyCustomField(IdentifiableName identifiable) : base(identifiable)
        {
        }

        public string CreateQueryString(string value)
        {
            return string.Format(MyCustomField.QUERY_FORMAT, Id, HttpUtility.UrlEncode(value));
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MyCustomField : MyCustomField<MyCustomFieldPossibleValue>
    {
        public static string MULTI_QUERY_FORMAT { get; } = "issue[custom_field_values][{0}][]={1}";
        public static string QUERY_FORMAT { get; } = "issue[custom_field_values][{0}]={1}";

        public MyCustomField()
        {
        }

        public MyCustomField(CustomField cf) : base(cf)
        {
            Format = cf.FieldFormat.ToCustomFieldFormat();
            // cf.FieldFormat が list や bool だった場合のみ PossibleValues に値が格納される
            if (cf.PossibleValues != null && cf.PossibleValues.Any())
                PossibleValues = cf.PossibleValues.Select(v => new MyCustomFieldPossibleValue(v)).ToList();
            else
                PossibleValues = new List<MyCustomFieldPossibleValue>();
        }

        /// <summary>
        /// for FieldFormat == user
        /// </summary>
        public MyCustomField(CustomField cf, List<MyUser> users) : this(cf)
        {
            PossibleValues = users.Select(u => new MyCustomFieldPossibleValue(u)).ToList();
        }

        /// <summary>
        /// for FieldFormat == version
        /// </summary>
        public MyCustomField(CustomField cf, List<MyProject> projects) : this(cf)
        {
            PossibleValues = projects.SelectMany(p => p.Versions.Select(v => new MyCustomFieldPossibleValue(p, v, projects.Count > 1))).ToList();
        }
    }

    public static class CustomFieldEx
    {
        public static bool IsIssueType(this CustomField c)
        {
            return c.CustomizedType == "issue";
        }

        public static bool IsBoolFormat(this CustomField c)
        {
            return c.FieldFormat == RedmineKeys.CF_BOOL;
        }

        public static bool IsListFormat(this CustomField c)
        {
            return c.FieldFormat == RedmineKeys.CF_LIST;
        }

        public static bool IsUserFormat(this CustomField c)
        {
            return c.FieldFormat == RedmineKeys.CF_USER;
        }

        public static bool IsVersionFormat(this CustomField c)
        {
            return c.FieldFormat == RedmineKeys.CF_VERSION;
        }

        public static bool CanUseVisualizeFactor(this CustomField c)
        {
            return c.IsIssueType() && (c.IsListFormat() || c.IsUserFormat() || c.IsVersionFormat());
        }

        /// <summary>
        /// カスタムフィールドが有効で、値が設定されているかどうかを返す
        /// </summary>
        public static bool HasValue(this IssueCustomField ic)
        {
            return ic.Values != null && ic.Values.Any(v => !string.IsNullOrEmpty(v.Info));
        }

        /// <summary>
        /// チケット一覧のグループ条件に設定できるかどうかを返す
        /// </summary>
        public static bool CanGroupBy(this CustomField c)
        {
            if (!c.IsIssueType())
                return false;

            switch (c.FieldFormat)
            {
                case RedmineKeys.CF_VERSION:     // バージョン
                case RedmineKeys.CF_USER:        // ユーザー
                case RedmineKeys.CF_LIST:        // リスト
                case RedmineKeys.CF_INT:         // 整数
                case RedmineKeys.CF_BOOL:        // 真偽値
                case RedmineKeys.CF_DATE:        // 日付
                case RedmineKeys.CF_ENUMERATION: // キーバリューリスト
                    return true;
                default:
                    return false;
            }
        }
    }
}
