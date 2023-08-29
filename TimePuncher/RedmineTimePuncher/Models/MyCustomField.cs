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
            if (Format == CustomFieldFormat.Bool || Format == CustomFieldFormat.List)
                PossibleValues = cf.PossibleValues.Select(v => new MyCustomFieldPossibleValue(v)).ToList();
            else if (Format == CustomFieldFormat.User)
                // Format が user の場合 cf.PossibleValues は null となる。
                // 現状、ユーザに PossibleValues を提示したい機能はないため空とする。
                PossibleValues = new List<MyCustomFieldPossibleValue>();
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
    }
}
