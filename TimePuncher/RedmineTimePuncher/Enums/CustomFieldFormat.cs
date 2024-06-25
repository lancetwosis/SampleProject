using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CustomFieldFormat
    {
        [LocalizedDescription(nameof(Resources.CustomFieldFormatBool), typeof(Resources))]
        Bool,
        [LocalizedDescription(nameof(Resources.CustomFieldFormatList), typeof(Resources))]
        List,
        [LocalizedDescription(nameof(Resources.CustomFieldFormatUser), typeof(Resources))]
        User,
        Version,
        // 他にも int, string, date, link などが存在するが現在対応する処理が存在しないため一括で扱う
        Other,
    }

    public static class CustomFieldFormatEx
    {
        public static CustomFieldFormat ToCustomFieldFormat(this string str)
        {
            switch (str)
            {
                case "bool":
                    return CustomFieldFormat.Bool;
                case "list":
                    return CustomFieldFormat.List;
                case "user":
                    return CustomFieldFormat.User;
                case "version":
                    return CustomFieldFormat.Version;
                default:
                    return CustomFieldFormat.Other;
            }
        }
    }
}
