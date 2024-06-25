using LibRedminePower.Enums;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Extentions
{
    public static class MarkupLangTypeEx
    {
        public static bool CanTranscribe(this MarkupLangType type)
        {
            switch (type)
            {
                case MarkupLangType.Textile:
                case MarkupLangType.Markdown:
                    return true;
                case MarkupLangType.Undefined:
                case MarkupLangType.None:
                default:
                    return false;
            }
        }

        public static string CreateParagraph(this MarkupLangType type, string header, params string[] rows)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                return string.Join(Environment.NewLine, rows);
            }

            var sb = new StringBuilder();
            switch (type)
            {
                case MarkupLangType.None:
                    sb.AppendLine($"- {header}");
                    rows.ToList().ForEach(r => sb.AppendLine($"  - {r}"));
                    break;
                case MarkupLangType.Textile:
                    sb.AppendLine($"h4. {header}");
                    sb.AppendLine();
                    rows.ToList().ForEach(r => sb.AppendLine(r));
                    break;
                case MarkupLangType.Markdown:
                    sb.AppendLine($"#### {header}");
                    sb.AppendLine();
                    // 改行を有効にするために「  」追加
                    rows.ToList().ForEach(r => sb.AppendLine($"{r}  "));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public static string CreateTicketLink(this MarkupLangType type, MyIssue issue)
        {
            switch (type)
            {
                case MarkupLangType.Textile:
                    return $"##{issue.Id}";
                case MarkupLangType.Markdown:
                    return $"[#{issue.Id} {issue.Subject}]({issue.Url})";
                case MarkupLangType.None:
                    return issue.Url;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string CreateLink(this MarkupLangType type, string label, string linkedUrl)
        {
            switch (type)
            {
                case MarkupLangType.Textile:
                    return $"\"{label}\":{linkedUrl}";
                case MarkupLangType.Markdown:
                    return $"[{label}]({linkedUrl})";
                case MarkupLangType.None:
                    return linkedUrl;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string GetHeaderPattern(this MarkupLangType type)
        {
            switch (type)
            {
                case MarkupLangType.Textile:
                    return HeaderLevelEx.TEXTILE_PATTERN;
                case MarkupLangType.Markdown:
                    return HeaderLevelEx.MARKDOWN_PATTERN;
                case MarkupLangType.None:
                default:
                    throw new InvalidOperationException();
            }
        }

        public static HeaderLevel GetHeaderLevel(this MarkupLangType type, string str)
        {
            switch (type)
            {
                case MarkupLangType.Textile:
                    return HeaderLevelEx.FromTextileHeader(str);
                case MarkupLangType.Markdown:
                    return HeaderLevelEx.FromMarkdownHeader(str);
                case MarkupLangType.None:
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string CreateCollapse(this MarkupLangType type, string header, string msg)
        {
            var sb = new StringBuilder();
            switch (type)
            {
                // Redmine の「テキスト書式」設定が Markdown でも同様に動作したので書式による分岐は行わない
                case MarkupLangType.Textile:
                case MarkupLangType.Markdown:
                    sb.AppendLine("{{collapse(" + header + ")");
                    sb.AppendLine(msg);
                    sb.AppendLine("}} ");
                    return sb.ToString();
                case MarkupLangType.None:
                    sb.AppendLine(header);
                    sb.AppendLine(msg);
                    return sb.ToString();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
