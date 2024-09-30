using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Custom;
using RedmineTableEditor.Models.TicketFields.Custom.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTableEditor.Extentions
{
    public static class CustomFieldExtentions
    {
        public static void Validate(this CustomField meta, string target)
        {
            if (meta.MinLength != null && meta.MinLength > target.Length)
                throw new ValidationException($"文字数{meta.MinLength} を不足しています。");
            if (meta.MaxLength != null && meta.MaxLength < target.Length)
                throw new ValidationException($"文字数{meta.MaxLength} を超過しています。");
            if (meta.Regexp != null && !Regex.IsMatch(target, meta.Regexp))
                throw new ValidationException($"文字列が正規表現 {meta.Regexp} に一致しません。");
        }

        /// <summary>
        /// カスタムフィールドの設定を使って IssueCustomField から FieldBase(CfBase) への変換を行う
        /// </summary>
        public static FieldBase ToFieldBase(this IssueCustomField cf, CustomField meta)
        {
            if (meta.Multiple)
            {
                switch (meta.FieldFormat)
                {
                    case "user":
                    case "version":
                        return new CfInts(meta, cf);
                    case "list":
                    case "enumeration":
                        return new CfStrings(meta, cf);
                    default:
                        throw new NotSupportedException($"Not supported for {meta.FieldFormat}, {meta.Multiple}");
                }
            }
            else
            {
                switch (meta.FieldFormat)
                {
                    case "string":
                    case "text":
                    case "link":
                    case "list":
                        return new CfString(meta, cf);
                    case "float":
                        return new CfFloat(meta, cf);
                    case "bool":
                        return new CfBool(meta, cf);
                    case "date":
                        return new CfDate(meta, cf);
                    case "int":
                    case "user":
                    case "version":
                    case "enumeration":
                        return new CfInt(meta, cf);
                    case "attachment":
                        return null;
                    default:
                        throw new NotSupportedException($"Not supported for {meta.FieldFormat}, {meta.Multiple}");
                }
            }
        }

        public static bool IsEnabled(this CustomField cf)
        {
            return cf.CustomizedType == "issue" && cf.Trackers != null && cf.Trackers.Count > 0;
        }
    }
}
