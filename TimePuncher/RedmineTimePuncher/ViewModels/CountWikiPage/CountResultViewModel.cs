using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Extentions;
using LibRedminePower.Localization;
using LibRedminePower.Helpers;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Reactive.Bindings;
using RedmineTimePuncher.Enums;

namespace RedmineTimePuncher.ViewModels.CountWikiPage
{
    public class CountResultViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public DateTime CountedAt { get; set; }
        public string Measurer { get; set; }
        public MyProject Project { get; set; }
        public string DisableWords { get; set; }
        public FilterType FilterType { get; set; }

        public MyWikiPage TopWikiPage { get; set; }

        public string DisablePageNames { get; set; }

        public string FileName => $"{Project.Name}_{TopWikiPage.Title}_{CountedAt.ToString("yyMMdd_HHmmss")}.csv";

        public CountResultViewModel(DateTime countedAt, string author, MyProject project, MyWikiPage parentWiki, FilterType type, string disableWords)
        {
            CountedAt = countedAt;
            Measurer = author;
            Project = project;
            TopWikiPage = parentWiki;
            DisableWords = disableWords;
            FilterType = type;

            TopWikiPage.CalculateSummary(disableWords, type);

            DisablePageNames = string.Join(", ", TopWikiPage.AllChildren.Flatten(w => w.AllChildren).Where(w => !w.IsSummaryTarget).Select(w => w.DisplayTitle).ToArray());
        }

        public void Export(string fileName)
        {
            var sb = new StringBuilder();

            sb.AppendLine(addDoubleQuat(Properties.Resources.WikiCountResultCountedAt, CountedAt));
            sb.AppendLine(addDoubleQuat(Properties.Resources.WikiCountResultMeasurer, Measurer));
            sb.AppendLine(addDoubleQuat(Properties.Resources.WikiCountResultProject, Project.Name));
            sb.AppendLine(addDoubleQuat(Properties.Resources.WikiCountResultParentPage, TopWikiPage.DisplayTitle));
            sb.AppendLine(addDoubleQuat(Properties.Resources.WikiCountResultExcludedPageNames, DisablePageNames));
            sb.AppendLine();
            sb.AppendLine(addDoubleQuat(Properties.Resources.WikiCountResultTitle,
                                        Properties.Resources.WikiCountResultParentPage,
                                        Properties.Resources.WikiCountResultUpdateOn,
                                        Properties.Resources.WikiCountResultAuthor,
                                        Properties.Resources.WikiCountResultNoOfChar,
                                        Properties.Resources.WikiCountResultNoOfLine,
                                        Properties.Resources.WikiCountResultNoOfCharIncluded,
                                        Properties.Resources.WikiCountResultNoOfLineIncluded));

            var wikis = new List<MyWikiPage>() { TopWikiPage }.Flatten(w => w.Children).ToList();
            foreach (var w in wikis)
            {
                sb.AppendLine(addDoubleQuat(w.IndexedTitle, w.ParentTitle, w.UpdatedOn, w.Author.Name, w.Summary.NoOfChar, w.Summary.NoOfLine,
                                            w.SummaryIncludedChildren.NoOfChar, w.SummaryIncludedChildren.NoOfLine));
            }

            FileHelper.WriteAllText(fileName, sb.ToString());
        }

        private string addDoubleQuat(params object[] strs)
        {
            return "\"" + string.Join("\",\"", strs) + "\"";
        }
    }
}
