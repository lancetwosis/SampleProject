using AngleSharp.Common;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using NetOffice.OutlookApi;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Managers;
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
using static NetOffice.OfficeApi.Tools.Contribution.DialogUtils;

namespace RedmineTimePuncher.Models
{
    public class MyWikiPage : MyWikiPageItem
    {
        public WikiPage RawWikiPage;

        // レビュー
        public MyWikiPage()
        { }

        public MyWikiPage(string urlBase, string projectId, WikiPage rawWikiPage, MyWikiPage parent = null) :base(urlBase, projectId, rawWikiPage)
        {
            this.RawWikiPage = rawWikiPage;
        }

        public List<WikiLine> GetSectionLines(MarkupLangType type, WikiLine header, bool includesHeader)
        {
            var lines = getLines();
            if (header.Equals(WikiLine.NOT_SPECIFIED))
                return lines;

            var startIndex = -1;
            var startHeaderLevel = HeaderLevel.None;
            var endIndex = lines.Count - 1;

            foreach (var line in lines)
            {
                if (!line.TryGetHeaderLevel(type, out var level))
                    continue;

                if (startIndex == -1 && line.Equals(header))
                {
                    startHeaderLevel = level;
                    startIndex = includesHeader ? line.LineNo - 1 : line.LineNo;
                }
                else
                {
                    if (startHeaderLevel >= level)
                    {
                        endIndex = line.LineNo - 2;
                        break;
                    }
                }
            }

            if (startHeaderLevel == HeaderLevel.None)
            {
                throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindHeader, header, Title));
            }

            // Textile の場合、ヘッダーのスタイルの適用を外すため、ヘッダーの次は必ず空行を入れる必要がある。
            // これは、ユーザが転記したい内容に関係がないため、取り除く。
            if (!includesHeader && string.IsNullOrWhiteSpace(lines[startIndex].Text))
                startIndex++;

            return lines.GetRange(startIndex, endIndex - startIndex + 1).ToList();
        }

        private List<WikiLine> getLines()
        {
            var lines = RawWikiPage.Text.SplitLines().Indexed().Select(i => new WikiLine(i.i + 1, i.v)).ToList();

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

        public List<WikiLine> GetHeaders(MarkupLangType type)
        {
            return getLines().Where(l => l.IsHeader(type)).ToList();
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
