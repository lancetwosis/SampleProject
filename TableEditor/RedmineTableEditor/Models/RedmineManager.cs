using LibRedminePower.Extentions;
using LibRedminePower.Interfaces;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models
{
    public class RedmineManager : LibRedminePower.Models.Bases.ModelBase
    {

        public string UrlBase => manager.Host;

        // クエリの選択が変更された時に更新
        public List<Tracker> Trackers { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public List<IdentifiableName> Users { get; set; }
        public List<Redmine.Net.Api.Types.Version> Versions { get; set; }
        public List<IssueCategory> Categories { get; set; }

        // Redmine の設定更新時に更新
        public List<IssueStatus> Statuses { get; set; }
        public List<IssuePriority> Priorities { get; set; }
        public List<Query> Queries { get; set; }
        public List<Project> Projects { get; set; }
        private List<Tracker> allTrackers { get; set; }
        private List<CustomField> allCustomFields { get; set; }

        private IRedmineManager manager { get; set; }
        private IRedmineManager masterManager { get; set; }
        private ICacheManager cashManager { get; set; }

        public RedmineManager((IRedmineManager Manager, IRedmineManager MasterManager, ICacheManager CashManager) redmine)
        {
            this.manager = redmine.Manager;
            this.masterManager = redmine.MasterManager;
            this.cashManager = redmine.CashManager;
        }

        public string IsValid()
        {
            if (manager == null)
                return Properties.Resources.ErrMsgSetRemineSetting;
            else if (masterManager == null)
                return Properties.Resources.ErrMsgSetAdminApiKey;
            else if (cashManager == null)
                return Properties.Resources.ErrMsgSetRemineSetting;
            else
                return null;
        }

        public void Update()
        {
            allTrackers = cashManager.Trackers.Value;
            allCustomFields = cashManager.CustomFields.Value
                    .Where(c => c.Trackers != null && c.Trackers.Any() && c.CustomizedType == "issue").ToList();
            Statuses = cashManager.Statuss.Value;
            Projects = cashManager.Projects.Value;
            Queries = cashManager.Queries.Value;
            Priorities = cashManager.Priorities.Value;
        }

        public async Task UpdateByQueryAsync(Query query)
        {
            await Task.Run(async () =>
            {
                if (query.ProjectId.HasValue)
                {
                    await updateByProjectIdAsync(query.ProjectId.Value);
                }
                else
                {
                    // 全プロジェクト向けのクエリだった場合、そのクエリで取得した Issue に紐づくすべてのプロジェクトを対象とする
                    var issues = GetIssues(query);
                    if (issues == null || issues.Count == 0)
                    {
                        Trackers = new List<Tracker>();
                        CustomFields = new List<CustomField>();
                        Users = new List<IdentifiableName>();
                        Versions = new List<Redmine.Net.Api.Types.Version>();
                        Categories = new List<IssueCategory>();
                        return;
                    }

                    var projIds = issues.Select(i => i.Project.Id).Distinct().ToList();
                    var projs = Projects.Where(p => projIds.Contains(p.Id)).ToList();
                    var trackerIds = projs.SelectMany(p => p.Trackers.Select(t => t.Id)).Distinct().ToList();

                    Trackers = allTrackers.Where(t => trackerIds.Contains(t.Id)).ToList();
                    CustomFields = allCustomFields.Where(c => c.Trackers.Any(t => Trackers.Any(t2 => t.Id == t2.Id))).ToList();

                    await Task.WhenAll(
                        Task.Run(async () =>
                        {
                            var memberships = await getItemsAsync<ProjectMembership>(projs, (i1, i2) => i1.Id == i2.Id);
                            Users = memberships != null && memberships.Any() ?
                                memberships.Select(m => m.User).Where(m => m != null).Distinct((u1, u2) => u1.Id == u2.Id).OrderBy(u => u.Id).ToList():
                                new List<IdentifiableName>();
                        }),
                        Task.Run(async () => Versions = await getItemsAsync<Redmine.Net.Api.Types.Version>(projs, (i1, i2) => i1.Id == i2.Id)),
                        Task.Run(async () => Categories = await getItemsAsync<IssueCategory>(projs, (i1, i2) => i1.Id == i2.Id)));
                }
            });
        }

        private async Task updateByProjectIdAsync(int projectId)
        {
            var proj = Projects.First(a => a.Id == projectId);

            Trackers = allTrackers.Where(t => proj.Trackers.Any(t2 => t2.Id == t.Id)).ToList();

            // Project の issue_custom_fields で判定したかったが、
            // 「全プロジェクト向け」に設定されているものが含まれないバグ（？）があるため、
            // プロジェクトで有効なトラッカーに紐づいているカスタムフィールドを使用する。
            // プロジェクトの設定でトラッカーが有効でも特定のカスタムフィールドが無効だった場合、
            // その無効なカスタムフィールドも表示されてしまう問題がある。
            // しかし、必要なカスタムフィールドが含まれないよりはマシだと判断し、この処理とする。
            // https://www.redmine.org/issues/38668
            CustomFields = allCustomFields.Where(c => c.Trackers.Any(t => Trackers.Any(t2 => t.Id == t2.Id))).ToList();

            await Task.WhenAll(
                Task.Run(() =>
                {
                    var memberships = manager.GetObjectsWithErrConv<ProjectMembership>(projectId);
                    Users = memberships != null && memberships.Any() ?
                        memberships.Select(m => m.User).Where(m => m != null).OrderBy(u => u.Id).ToList() :
                        new List<IdentifiableName>();
                }),
                Task.Run(() => Versions = manager.GetObjectsWithErrConv<Redmine.Net.Api.Types.Version>(projectId)),
                Task.Run(() => Categories = manager.GetObjectsWithErrConv<IssueCategory>(projectId)));
        }

        private async Task<List<T>> getItemsAsync<T>(List<Project> projs, Func<T, T, bool> isSame) where T : class, new()
        {
            var itemsLists = await Task.WhenAll(projs.Select(p => Task.Run(() => manager.GetObjectsWithErrConv<T>(p.Id))));
            return itemsLists.Where(l => l != null).SelectMany(l => l).Distinct(isSame).ToList();
        }

        public async Task UpdateByParentIssueAsync(Issue issue)
        {
            await Task.Run(async () =>
            {
                await updateByProjectIdAsync(issue.Project.Id);
            });
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

        public List<Issue> GetIssues(IEnumerable<int> ids)
        {
            return manager.GetObjectsWithErrConv<Issue>(new NameValueCollection
                {
                    { RedmineKeys.ISSUE_ID, string.Join(",", ids) },
                    { RedmineKeys.STATUS_ID, "*" },
                });
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

        public void UpdateTicket(Issue issue)
        {
            manager.UpdateObjectWithErrConv(issue.Id.ToString(), issue);
        }
    }
}
