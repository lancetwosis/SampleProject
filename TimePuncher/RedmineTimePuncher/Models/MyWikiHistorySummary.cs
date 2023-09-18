using AngleSharp.Dom;
using DiffPlex.DiffBuilder;
using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyWikiHistorySummary
    {
        public string Title { get; set; }
        public int Version { get; set; }
        public string Url => $"{url}/{Version}";
        public DateTime? UpdatedOn { get; set; }
        public IdentifiableName Author { get; set; }
        public string Comment { get; set; }
        public int InsertNoOfLine { get; set; }
        public int InsertNoOfChar { get; set; }
        public int DeleteNoOfLine { get; set; }
        public int DeleteNoOfChar { get; set; }
        public MyWikiSummary Summary { get; set; }

        private string url;

        public MyWikiHistorySummary(string url, WikiPage h)
        {
            this.url = url;
            Title = h.Title;
            Version = h.Version;
            UpdatedOn = h.UpdatedOn;
            Author = h.Author;
            Comment = h.Comments;
            Summary = new MyWikiSummary(h);
            InsertNoOfLine = h.Text.SplitLines().Where(b => !string.IsNullOrEmpty(b)).Count();
            InsertNoOfChar = h.Text.Length;
        }

        public MyWikiHistorySummary(string url, WikiPage prev, WikiPage next) : this(url, next)
        {
            var diff = InlineDiffBuilder.Diff(prev.Text, next.Text);

            var inserts = diff.Lines.Where(a => a.Type == DiffPlex.DiffBuilder.Model.ChangeType.Inserted).ToArray();
            var deletes = diff.Lines.Where(a => a.Type == DiffPlex.DiffBuilder.Model.ChangeType.Inserted).ToArray();

            InsertNoOfLine = inserts.Sum(a => a.Text.SplitLines().Where(b => !string.IsNullOrEmpty(b)).Count());
            InsertNoOfChar = inserts.Sum(a => a.Text.Length);
            DeleteNoOfLine = deletes.Sum(a => a.Text.SplitLines().Where(b => !string.IsNullOrEmpty(b)).Count());
            DeleteNoOfChar = deletes.Sum(a => a.Text.Length);
        }
    }
}
