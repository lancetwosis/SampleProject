using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel;
using System.Reactive.Linq;
using LibRedminePower.Extentions;

namespace RedmineTimePuncher.Models
{
    public class MyWikiPageCount : MyWikiPageItem
    {
        public int Depth { get; set; }
        public string IndexedTitle => getIndexedTitle();
        [JsonIgnore]
        public ObservableCollection<MyWikiPageCount> Children { get; set; }
        public MyWikiSummary Summary { get; set; }
        [JsonIgnore]
        public MyWikiSummary SummaryIncludedChildren { get; set; }
        public BusyNotifier IsBusyUpdateSummary { get; set; }

        public CancellationTokenSource CtsUpdateHistories { get; set; }
        public BusyNotifier IsBusyUpdateHistories { get; set; }
        [JsonIgnore]
        public List<MyWikiHistorySummary> Histories { get; set; }

        public MyWikiPageCount Parent { get; set; }

        private (DateTime, DateTime) startEnd;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public MyWikiPageCount(MyWikiPageItem item, MyWikiPageCount parent, List<MyWikiPageItem> allWikis) :base(item)
        {
            this.Parent = parent;
            Depth = parent == null ? 0 : parent.Depth + 1;

            Children = new ObservableCollection<MyWikiPageCount>();

            IsBusyUpdateSummary = new BusyNotifier();
            IsBusyUpdateSummary.Where(a => !a).SubscribeWithErr(_ => 
            {
                if (Children.Any())
                {
                    SummaryIncludedChildren = new MyWikiSummary(Children.Select(c => c.SummaryIncludedChildren).ToList());
                }
                else
                {
                    SummaryIncludedChildren = Summary;
                }
            });
            IsBusyUpdateHistories = new BusyNotifier();

            foreach (var wiki in allWikis.Where(w => w.ParentTitle == Title))
            {
                var child = new MyWikiPageCount(wiki, this, allWikis);
                Children.Add(child);
            }
        }

        public async Task UpdateSummaryAsync(RedmineManager redmine)
        {
            using (IsBusyUpdateSummary.ProcessStart())
            {
                var wiki = await Task.Run(() => redmine.GetWikiPage(ProjectId, Title));
                Author = wiki.Author;
                Summary = new MyWikiSummary(wiki.RawWikiPage);
                await Task.WhenAll(Children.Select(a => a.UpdateSummaryAsync(redmine)));
            }
        }

        public async Task UpdateHistoriesAsync(RedmineManager redmine, (DateTime, DateTime)? startEndTarget)
        {
            if (!startEndTarget.HasValue)
            {
                Histories = null;
                return;
            }
            var startEnd = startEndTarget.Value;

            using (IsBusyUpdateHistories.ProcessStart())
            {
                try
                {
                    if (Histories != null && this.startEnd.Item1 == startEnd.Item1 && this.startEnd.Item2 == startEnd.Item2) return;
                    CtsUpdateHistories = new CancellationTokenSource();
                    this.startEnd = startEnd;

                    //await semaphore.WaitAsync(CtsUpdateHistories.Token);

                    Histories = new List<MyWikiHistorySummary>();

                    var histories = new List<MyWikiPage>();
                    foreach (var v in Enumerable.Range(1, Version).Reverse().Indexed())
                    {
                        // 変更履歴バージョンを取得する。
                        var h = await Task.Run(() => redmine.GetWikiPage(ProjectId, Title, v.v));
                        if (!v.isFirst)
                        {
                            // ひとつ前と比較して、変更量を算出する。
                            Histories.Add(new MyWikiHistorySummary(Url, h.RawWikiPage, histories.Last().RawWikiPage));

                            if (startEnd.Item1 > h.UpdatedOn) break;
                            if (CtsUpdateHistories.IsCancellationRequested) return;
                        }

                        // 初版の場合
                        if (Version == 1)
                            Histories.Add(new MyWikiHistorySummary(Url, h.RawWikiPage));

                        histories.Add((h));

                    }
                }
                catch(Exception) 
                {
                    Histories = null;
                }
                finally
                {
                    //semaphore.Release();
                }
            }
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

        private string createPrefix(MyWikiPageCount child, string prefix)
        {
            if (child.Parent != null)
            {
                var isLastChild = child.Parent.Children.Last().Title == child.Title;
                prefix = isLastChild ? $"   {prefix}" : $"|  {prefix}";
                prefix = createPrefix(child.Parent, prefix);
            }

            return prefix;
        }

        public override string ToString()
        {
            var summary = Summary != null ? $", {Summary}" : "";
            return $"{Title}{summary}";
        }
    }
}
