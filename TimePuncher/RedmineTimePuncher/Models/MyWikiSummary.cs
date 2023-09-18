using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyWikiSummary
    {
        public int NoOfLine { get; set; }
        public int NoOfChar { get; set; }
        public int NoOfHeader1 { get; set; }
        public int NoOfHeader2 { get; set; }
        public int NoOfHeader3 { get; set; }

        public MyWikiSummary(WikiPage rawWikiPage)
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

        public MyWikiSummary(List<MyWikiSummary> sumaries)
        {
            NoOfLine = sumaries.Sum(s => s.NoOfLine);
            NoOfChar = sumaries.Sum(s => s.NoOfChar);
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
