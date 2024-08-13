using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using Redmine.Net.Api;
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
        Int,
        String,
        Float,
        Link,
        Text,
        Date,
        Enumeration,
        Attachment,
        Other,
    }

    public static class CustomFieldFormatEx
    {
        public static CustomFieldFormat ToCustomFieldFormat(this string str)
        {
            switch (str)
            {
                case RedmineKeys.CF_BOOL:
                    return CustomFieldFormat.Bool;
                case RedmineKeys.CF_LIST:
                    return CustomFieldFormat.List;
                case RedmineKeys.CF_USER:
                    return CustomFieldFormat.User;
                case RedmineKeys.CF_VERSION:
                    return CustomFieldFormat.Version;
                case RedmineKeys.CF_INT:
                    return CustomFieldFormat.Int;
                case RedmineKeys.CF_STRING:
                    return CustomFieldFormat.String;
                case RedmineKeys.CF_FLOAT:
                    return CustomFieldFormat.Float;
                case RedmineKeys.CF_LINK:
                    return CustomFieldFormat.Link;
                case RedmineKeys.CF_TEXT:
                    return CustomFieldFormat.Text;
                case RedmineKeys.CF_DATE:
                    return CustomFieldFormat.Date;
                case RedmineKeys.CF_ENUMERATION:
                    return CustomFieldFormat.Enumeration;
                case RedmineKeys.CF_ATTACHMENT:
                    return CustomFieldFormat.Attachment;
                default:
                    return CustomFieldFormat.Other;
            }
        }
    }
}
