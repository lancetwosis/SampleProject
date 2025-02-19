using AngleSharp.Common;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyWikiPage : MyWikiPageItem
    {
        public WikiPage RawWikiPage;

        [Obsolete("For Serialize", true)]
        public MyWikiPage()
        { }

        public MyWikiPage(string urlBase, string projectId, WikiPage rawWikiPage, MyWikiPage parent = null) :base(urlBase, projectId, rawWikiPage)
        {
            this.RawWikiPage = rawWikiPage;
        }

        public string GetSection(TranscribeSettingItemModel setting, MarkupLangType type, List<Project> projects, RedmineManager redmine)
        {
            var lines = GetLines(type);
            if (setting.Header.Equals(WikiLine.NOT_SPECIFIED))
                return string.Join(Environment.NewLine, lines.SelectMany(l => expandLine(Title, l, setting.ExpandsIncludeMacro, type, projects, redmine)).Select(l => l.Text));

            var headerLevel = HeaderLevel.None;
            var results = new List<WikiLine>();
            foreach (var line in lines)
            {
                var level = line.GetHeaderLevel(type);
                if (level != HeaderLevel.None)
                {
                    if (headerLevel == HeaderLevel.None && line.Equals(setting.Header))
                    {
                        headerLevel = level;
                        results.Add(line);
                    }
                    else
                    {
                        if (headerLevel >= level)
                            break;

                        results.Add(line);
                    }
                }
                else
                {
                    if (headerLevel != HeaderLevel.None)
                    {
                        results.AddRange(expandLine(Title, line, setting.ExpandsIncludeMacro, type, projects, redmine));
                    }
                }
            }

            if (headerLevel == HeaderLevel.None)
            {
                throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindHeader, setting.Header, Title));
            }

            if (!setting.IncludesHeader)
            {
                if (type == MarkupLangType.Markdown)
                {
                    results.RemoveAt(0);
                }
                else if (type == MarkupLangType.Textile)
                {
                    // Textile の場合、ヘッダーのスタイルの適用を外すため、ヘッダーの次は必ず空行を入れる必要がある。
                    // これは、ユーザが転記したい内容に関係がないため、取り除く。
                    results.RemoveAt(0);
                    results.RemoveAt(0);
                }
            }

            return string.Join(Environment.NewLine, results.Select(l => l.Text));
        }

        public List<WikiLine> GetLines(MarkupLangType type)
        {
            var lines = new List<WikiLine>();
            List<WikiLine> codeBlocks = null;
            foreach (var i in RawWikiPage.Text.SplitLines().Indexed())
            {
                var line = new WikiLine(i.i + 1, i.v);
                if (i.v.Contains(type.GetCodeBlockStart()))
                {
                    codeBlocks = new List<WikiLine>() { line };
                }
                else if (codeBlocks != null)
                {
                    codeBlocks.Add(line);
                    if (i.v.Contains(type.GetCodeBlockEnd()))
                    {
                        codeBlocks.ForEach(c => c.InCodeBlock = true);
                        codeBlocks = null;
                    }
                }
                lines.Add(line);
            }

            foreach (var sameLines in lines.GroupBy(h => h.Text))
            {
                var i = 1;
                foreach (var l in sameLines)
                {
                    l.RepeatCount = i;
                    i++;
                }
            }

            // Wiki から取得した文字列のラストに以下の文字列が含まれることがある。
            // ユーザが設定したものではないので削除する。
            if (lines.Count > 3)
            {
                var last3 = lines[lines.Count - 3];
                var last2 = lines[lines.Count - 2];
                var last1 = lines[lines.Count - 1];
                if (last3.Text == "" &&
                    last2.Text == "{{fnlist}}" &&
                    last1.Text == "")
                {
                    lines.RemoveAt(lines.Count - 1);
                    lines.RemoveAt(lines.Count - 1);
                    lines.RemoveAt(lines.Count - 1);
                }
            }

            return lines;
        }

        private List<WikiLine> expandLine(string wikiName, WikiLine line, bool expandsIncludeMacro, MarkupLangType type,
            List<Project> projects, RedmineManager redmine, List<string> includeWikiUrls = null)
        {
            if (line.InCodeBlock)
                return new List<WikiLine>() { line };

            var path = line.GetImagePath(type);
            if (path != null)
                return convertImageLine(line, path);

            if (!expandsIncludeMacro)
                return new List<WikiLine>() { line };

            var results = new List<WikiLine>();
            var include = Regex.Match(line.Text, @"{{include\((.+)\)}}");
            if (include.Success)
            {
                results.AddRange(expandIncludeMacro(wikiName, include.Groups[1].ToString(), type, projects, redmine, includeWikiUrls ?? new List<string>()));
            }
            else
            {
                results.Add(line);
            }
            return results;
        }

        private List<WikiLine> convertImageLine(WikiLine line, string path)
        {
            if (!path.Contains("/"))
            {
                var attach = RawWikiPage.Attachments.FirstOrDefault(a => a.FileName == path);
                if (attach != null)
                {
                    line.Text = line.Text.Replace(path, attach.ContentUrl);
                }
            }
            return new List<WikiLine>() { line };
        }

        private List<WikiLine> expandIncludeMacro(string srcWikiName, string macroPrms, MarkupLangType type,
            List<Project> projects, RedmineManager redmine, List<string> includeWikiUrls)
        {
            MyWikiPage included = null;
            try
            {
                var prms = Regex.Match(macroPrms, @"^(.+):(.+)$");
                if (prms.Success)
                {
                    // Redmine のマクロでは Identifier の大文字小文字の区別しないため以下のようにする
                    var project = projects.First(p => p.Identifier.ToLower() == prms.Groups[1].ToString().ToLower());
                    included = redmine.GetWikiPage(project.Id.ToString(), prms.Groups[2].ToString());
                }
                else
                {
                    included = redmine.GetWikiPage(ProjectId, macroPrms);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedExpandInclude, srcWikiName, "{{include(" + macroPrms + ")}}"), e);
            }

            if (includeWikiUrls.Contains(included.Url))
            {
                // 循環して参照してしまうのを避ける
                return new List<WikiLine>();
            }
            else
            {
                includeWikiUrls.Add(included.Url);
                return included.GetLines(type).SelectMany(l => expandLine(included.Title, l, true, type, projects, redmine, includeWikiUrls)).ToList();
            }
        }

        public List<WikiLine> GetHeaders(MarkupLangType type)
        {
            return GetLines(type).Where(l => l.IsHeader(type)).ToList();
        }

        public override bool Equals(object obj)
        {
            return obj is MyWikiPage page &&
                   ProjectId == page.ProjectId &&
                   Title == page.Title;
        }

        public override int GetHashCode()
        {
            int hashCode = -167644508;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            return hashCode;
        }
    }
}
