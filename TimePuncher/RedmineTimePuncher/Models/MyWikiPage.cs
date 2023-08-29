using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyWikiPage : LibRedminePower.Models.Bases.ModelBase
    {
        public string Url { get; set; }
        public string ProjectId { get; set; }
        public string Title { get; set; }
        public string DisplayTitle => IsTopWiki ? $"{Title} ({Properties.Resources.WikiCountTopPage})" : Title;
        public string IndexedTitle => getIndexedTitle();
        public int Version { get; set; }
        public IdentifiableName Author { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public string ParentTitle { get; set; }
        [JsonIgnore]
        public ObservableCollection<MyWikiPage> AllChildren { get; set; }
        [JsonIgnore]
        public IFilteredReadOnlyObservableCollection<MyWikiPage> Children { get; set; }
        public bool IsSummaryTarget { get; set; } = true;
        [JsonIgnore]
        public Summary Summary { get; set; }
        [JsonIgnore]
        public Summary SummaryIncludedChildren { get; set; }

        public bool IsTopWiki { get; set; }
        public int Depth { get; set; }

        public MyWikiPage Parent { get; set; }

        private WikiPage rawWikiPage;

        public MyWikiPage() { }

        public MyWikiPage(string urlBase, string projectId, WikiPage rawWikiPage, MyWikiPage parent = null)
        {
            this.Parent = parent;
            this.rawWikiPage = rawWikiPage;

            Url = $"{urlBase}projects/{projectId}/wiki/{rawWikiPage.Title}";
            ProjectId = projectId;
            Title = rawWikiPage.Title;
            CreatedOn = rawWikiPage.CreatedOn;
            UpdatedOn = rawWikiPage.UpdatedOn;
            Author = rawWikiPage.Author;
            Version = rawWikiPage.Version;

            IsTopWiki = Title == "Wiki";
            Depth = parent == null ? 0 : parent.Depth + 1;

            ParentTitle = rawWikiPage.ParentTitle;
            AllChildren = new ObservableCollection<MyWikiPage>();
            Children = AllChildren.ToFilteredReadOnlyObservableCollection(c => c.IsSummaryTarget);
        }

        public void CalculateSummary(string disableWords, FilterType type)
        {
            IsSummaryTarget = (Parent == null || Parent.IsSummaryTarget) ?
                  string.IsNullOrEmpty(disableWords) ? true : !Title.IsMatch(type, disableWords) :
                  false;

            Summary = new Summary(rawWikiPage);

            if (!IsSummaryTarget)
                return;

            if (AllChildren.Any())
            {
                foreach (var c in AllChildren)
                {
                    c.CalculateSummary(disableWords, type);
                }

                var summaries = AllChildren.Where(c => c.IsSummaryTarget).Select(c => c.SummaryIncludedChildren).ToList();
                summaries.Add(Summary);
                SummaryIncludedChildren = new Summary(summaries);
            }
            else
            {
                SummaryIncludedChildren = Summary;
            }
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
            var lines = rawWikiPage.Text.SplitLines().Indexed().Select(i => new WikiLine(i.i + 1, i.v)).ToList();

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

        public override string ToString()
        {
            var summary = Summary != null ? $", {Summary}" : "";
            return $"{Title}{summary}";
        }

        private string getIndexedTitle()
        {
            if (Depth == 0)
                return Title;

            var isLastChild = Parent.Children.Last().Title == this.Title;
            var prefix = isLastChild ? "`- " : "|- ";

            prefix = createPrefix(Parent, prefix);

            return $"{prefix}{Title}";
        }

        private string createPrefix(MyWikiPage child, string prefix)
        {
            if (child.Parent != null)
            {
                var isLastChild = child.Parent.Children.Last().Title == child.Title;
                prefix = isLastChild ? $"   {prefix}" : $"|  {prefix}";
                prefix = createPrefix(child.Parent, prefix);
            }

            return prefix;
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

        public void GoToWiki()
        {
            System.Diagnostics.Process.Start(Url);
        }
    }

    public class Summary
    {
        public int NoOfChar { get; set; }
        public int NoOfLine { get; set; }
        public int NoOfHeader1 { get; set; }
        public int NoOfHeader2 { get; set; }
        public int NoOfHeader3 { get; set; }

        public Summary(WikiPage rawWikiPage)
        {
            if (rawWikiPage.Text == null)
                return;

            NoOfChar = rawWikiPage.Text.Length;

            var lines = rawWikiPage.Text.SplitLines();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    NoOfLine++;
                    if (line.Contains("h1. "))
                        NoOfHeader1++;
                    else if (line.Contains("h2. "))
                        NoOfHeader2++;
                    else if (line.Contains("h3. "))
                        NoOfHeader3++;
                }
            }
        }

        public Summary(List<Summary> sumaries)
        {
            NoOfChar = sumaries.Sum(s => s.NoOfChar);
            NoOfLine = sumaries.Sum(s => s.NoOfLine);
            NoOfHeader1 = sumaries.Sum(s => s.NoOfHeader1);
            NoOfHeader2 = sumaries.Sum(s => s.NoOfHeader2);
            NoOfHeader3 = sumaries.Sum(s => s.NoOfHeader3);
        }

        public override string ToString()
        {
            return $"chars: {NoOfChar}, lines: {NoOfLine}";
        }
    }
}
