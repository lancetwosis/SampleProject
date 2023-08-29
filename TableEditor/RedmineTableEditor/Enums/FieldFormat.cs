using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTableEditor.Enums
{
    public enum FieldFormat
    {
        @string,
        @text,
        @link,
        @float,
        @bool,
        @int,
        @user,
        @user_multi,
        @version,
        @version_multi,
        @date,
        @list,
        @list_multi,
        // キー・バリューリスト（単数）
        @enumeration,
        // キー・バリューリスト（複数）
        @enumeration_multi,
        // ファイル
        attachment,
    }

    public static class FieldFormatEx
    {
        public static bool IsSupported(this FieldFormat cf)
        {
            switch (cf)
            {
                case FieldFormat.@string:
                case FieldFormat.text:
                case FieldFormat.link:
                case FieldFormat.@float:
                case FieldFormat.@bool:
                case FieldFormat.@int:
                case FieldFormat.user:
                case FieldFormat.user_multi:
                case FieldFormat.version:
                case FieldFormat.version_multi:
                case FieldFormat.date:
                case FieldFormat.list:
                case FieldFormat.list_multi:
                case FieldFormat.enumeration:
                case FieldFormat.enumeration_multi:
                    return true;
                case FieldFormat.attachment:
                    return false;
                default:
                    throw new InvalidProgramException();
            }
        }

        public static FieldFormat ToFieldFormat(this CustomField cf)
        {
            switch (cf.FieldFormat)
            {
                case "link":        return FieldFormat.link;
                case "string":      return FieldFormat.@string;
                case "text":        return FieldFormat.@text;
                case "float":       return FieldFormat.@float;
                case "bool":        return FieldFormat.@bool;
                case "int":         return FieldFormat.@int;
                case "date":        return FieldFormat.@date;
                case "user":        return cf.Multiple ? FieldFormat.@user_multi : FieldFormat.@user;
                case "version":     return cf.Multiple ? FieldFormat.version_multi : FieldFormat.@version;
                case "list":        return cf.Multiple ? FieldFormat.@list_multi : FieldFormat.@list;
                case "enumeration": return cf.Multiple ? FieldFormat.@enumeration_multi: FieldFormat.@enumeration;
                case "attachment":  return FieldFormat.attachment;
                default:
                    throw new InvalidProgramException();
            }
        }

        public static TextAlignment GetTextAlignment(this FieldFormat format)
        {
            switch (format)
            {
                case FieldFormat.@int:
                case FieldFormat.@float:
                    return TextAlignment.Right;
                case FieldFormat.@string:
                case FieldFormat.text:
                case FieldFormat.link:
                case FieldFormat.date:
                case FieldFormat.list:
                case FieldFormat.@bool:
                case FieldFormat.user:
                case FieldFormat.version:
                case FieldFormat.enumeration:
                default:
                    return TextAlignment.Left;
            }
        }

        public static string GetDataFormatString(this FieldFormat format)
        {
            switch (format)
            {
                case FieldFormat.@float:
                    return "N2";
                case FieldFormat.date:
                    return "yyyy/MM/dd";
                case FieldFormat.@string:
                case FieldFormat.text:
                case FieldFormat.link:
                case FieldFormat.list:
                case FieldFormat.@bool:
                case FieldFormat.@int:
                case FieldFormat.user:
                case FieldFormat.version:
                case FieldFormat.enumeration:
                default:
                    return null;
            }
        }
    }
}
