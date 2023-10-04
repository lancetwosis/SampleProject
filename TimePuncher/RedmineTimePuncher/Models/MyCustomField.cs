using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
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
    public class MyCustomField : IdName
    {
        public CustomFieldFormat Format { get; set; }
        public List<MyCustomFieldPossibleValue> PossibleValues { get; set; }

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

        public string CreateQueryString(string value)
        {
            return $"issue[custom_field_values][{Id}]={HttpUtility.UrlEncode(value)}";
        }
    }

    public static class CustomFieldEx
    {
        public static bool IsBoolFormat(this CustomField c)
        {
            return c.CustomizedType == "issue" && c.FieldFormat == "bool";
        }

        public static bool IsListFormat(this CustomField c)
        {
            return c.CustomizedType == "issue" && c.FieldFormat == "list";
        }

        public static bool IsUserFormat(this CustomField c)
        {
            return c.CustomizedType == "issue" && c.FieldFormat == "user";
        }

        public static bool IsVersionFormat(this CustomField c)
        {
            return c.CustomizedType == "issue" && c.FieldFormat == "version";
        }

        public static bool CanUseVisualizeFactor(this CustomField c)
        {
            return c.IsListFormat() || c.IsUserFormat() || c.IsVersionFormat();
        }

        /// <summary>
        /// カスタムフィールドが有効で、値が設定されているかどうかを返す
        /// </summary>
        public static bool HasValue(this IssueCustomField ic)
        {
            return ic.Values != null && ic.Values.Any(v => !string.IsNullOrEmpty(v.Info));
        }
    }
}
