using LibRedminePower.Extentions;
using LibRedminePower.Interfaces;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models
{
    public class RedmineManager : LibRedminePower.Models.Bases.ModelBaseSlim
    {

        public string UrlBase => manager.Host;

        // クエリの選択が変更された時に更新
        public List<Tracker> Trackers { get; set; }
        public List<IdentifiableName> Users { get; set; }
        public List<Redmine.Net.Api.Types.Version> Versions { get; set; }
        public List<IssueCategory> Categories { get; set; }

        private IRedmineManager manager { get; set; }
        private IRedmineManager masterManager { get; set; }
        public ICacheManager Cache { get; set; }


        public RedmineManager((IRedmineManager Manager, IRedmineManager MasterManager, ICacheManager CacheManager) redmine)
        {
            this.manager = redmine.Manager;
            this.masterManager = redmine.MasterManager;

            this.Cache = redmine.CacheManager;
        }

        public string IsValid()
        {
            if (manager == null)
                return Properties.Resources.ErrMsgSetRemineSetting;
            else if (masterManager == null)
                return Properties.Resources.ErrMsgSetAdminApiKey;
            else
                return null;
        }

        public async Task UpdateAsync(List<Issue> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                Trackers = new List<Tracker>();
                Users = new List<IdentifiableName>();
                Versions = new List<Redmine.Net.Api.Types.Version>();
                Categories = new List<IssueCategory>();
                return;
            }

            var projIds = issues.Select(i => i.Project.Id).Distinct().ToList();
            var projs = Cache.Projects.Where(p => projIds.Contains(p.Id)).ToList();
            var trackerIds = projs.SelectMany(p => p.Trackers.Select(t => t.Id)).Distinct().ToList();

            Trackers = Cache.Trackers.Where(t => trackerIds.Contains(t.Id)).ToList();
            Versions = projIds.Where(id => Cache.ProjectVersions.ContainsKey(id))
                .SelectMany(id => Cache.ProjectVersions[id])
                .ToList();
            Users = projIds.Where(id => Cache.ProjectMemberships.ContainsKey(id))
                .SelectMany(id => Cache.ProjectMemberships[id].Select(m => m.User).Where(a => a != null))
                .Distinct((u1, u2) => u1.Id == u2.Id).OrderBy(u => u.Id).ToList();
            await Task.Run(async () => Categories = await getItemsAsync<IssueCategory>(projs, (i1, i2) => i1.Id == i2.Id));
        }

        private async Task<List<T>> getItemsAsync<T>(List<Project> projs, Func<T, T, bool> isSame) where T : class, new()
        {
            var itemsLists = await Task.WhenAll(projs.Select(p => Task.Run(() => manager.GetObjectsWithErrConv<T>(p.Id))));
            return itemsLists.Where(l => l != null).SelectMany(l => l).Distinct(isSame).ToList();
        }

        /// <summary>
        /// 指定されたチケットとその子チケットを再帰的に対象として、それらに紐づけられた作業時間を取得する
        /// </summary>
        public List<TimeEntry> GetTimeEntries(int issueId)
        {
            return manager.GetObjectsWithErrConv<TimeEntry>(
                // ~ をつけることで子チケットを再帰的に検索してくれる
                new NameValueCollection { { RedmineKeys.ISSUE_ID, "~" + issueId.ToString() } });
        }

        public Issue GetIssue(int id)
        {
            return manager.GetObjectWithErrConv<Issue>(id.ToString());
        }

        private ConcurrentBag<Issue> issuesCache { get; set; }
        private ConcurrentBag<Issue> issuesCacheWithDetail { get; set; }
        /// <summary>
        /// 短時間で連続して取得する際、同一チケットへの API 実行を避けるためのキャッシュを初期化する。
        /// GetIssueFromCache を使った処理の前に実行すること。
        /// </summary>
        public void InitCaches(List<Issue> issues = null)
        {
            issuesCache = issues == null ? new ConcurrentBag<Issue>() : new ConcurrentBag<Issue>(issues);
            issuesCacheWithDetail = new ConcurrentBag<Issue>();
        }

        /// <summary>
        /// 短時間に連続でチケットを取得するときに、同一チケットへの API 実行を避けるため使用する。
        /// 実行前に InitCaches を実行すること。
        /// </summary>
        public Issue GetIssueFromCache(int id, bool needsDetail)
        {
            if (needsDetail)
            {
                var result = issuesCacheWithDetail.FirstOrDefault(i => i.Id == id);
                if (result != null)
                    return result;
            }
            else
            {
                var result = issuesCache.FirstOrDefault(i => i.Id == id);
                if (result != null)
                    return result;
            }

            var issue = GetIssue(id);
            issuesCache.Add(issue);
            issuesCacheWithDetail.Add(issue);
            return issue;
        }

        public List<Issue> GetIssues(IEnumerable<int> ids)
        {
            return manager.GetObjectsWithErrConv<Issue>(new NameValueCollection
                {
                    { RedmineKeys.ISSUE_ID, string.Join(",", ids) },
                    { RedmineKeys.STATUS_ID, "*" },
                });
        }

        public List<Issue> GetIssues(NameValueCollection parameters, List<string> additionalQueries)
        {
            return manager.GetObjectsWithErrConv<Issue>(parameters, additionalQueries);
        }

        public List<Issue> GetIssues(Query query)
        {
            if (query.ProjectId.HasValue)
            {
                return manager.GetObjectsWithErrConv<Issue>(new NameValueCollection
                {
                    { RedmineKeys.QUERY_ID, $"{query.Id}" },
                    { RedmineKeys.PROJECT_ID, $"{query.ProjectId}" },
                });
            }
            else
            {
                return manager.GetObjectsWithErrConv<Issue>(new NameValueCollection
                {
                    { RedmineKeys.QUERY_ID, $"{query.Id}" }
                });
            }
        }

        public string GetIssueUrl(int id)
        {
            return $"{UrlBase}issues/{id}";
        }

        /// <summary>
        /// 指定されたチケットの子チケットを再帰的に取得する。
        /// </summary>
        public List<Issue> GetChildIssues(int parentIssueId, bool recursive = true, bool includeSelf = false)
        {
            var results = new List<Issue>();

            if (includeSelf)
                results.Add(GetIssue(parentIssueId));

            // ~ をつけることで子チケットを再帰的に検索してくれる
            var id = recursive ? $"~{parentIssueId}" : $"{parentIssueId}";
            var children = manager.GetObjectsWithErrConv<Issue>(new NameValueCollection
                {
                    { RedmineKeys.PARENT_ID, id },
                    { RedmineKeys.STATUS_ID, "*" }
                });
            if (children != null)
                results.AddRange(children);

            return results;
        }

        public Issue GetIssueIncludeJournals(int id)
        {
            var issue = manager.GetObjectWithErrConv<Issue>(id.ToString(), new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS } });

            // キャッシュが有効の場合、追加する
            if (issuesCache != null)
                issuesCache.Add(issue);
            if (issuesCacheWithDetail != null)
                issuesCacheWithDetail.Add(issue);

            return issue;
        }

        public void UpdateTicket(Issue issue)
        {
            manager.UpdateObjectWithErrConv(issue.Id.ToString(), issue);
        }
    }
}
