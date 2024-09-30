using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Models.Settings;
using System.Text.Json.Serialization;
using Redmine.Net.Api;
using System.Collections.Specialized;
using Reactive.Bindings.Notifiers;
using LibRedminePower.Enums;
using Redmine.Net.Api.Types;
using Reactive.Bindings;
using LibRedminePower.Helpers;
using LibRedminePower.Interfaces;
using LibRedminePower.Logging;
using Reactive.Bindings.Extensions;
using System.Threading;
using System.Reactive.Linq;
using System.Collections.Concurrent;
using LibRedminePower.Exceptions;
using Redmine.Net.Api.Exceptions;
using LibRedminePower.Models;

namespace RedmineTimePuncher.Models.Managers
{
    public class CacheManager : ICacheManager
    {
        [JsonIgnore]
        public static CacheManager Default { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<DateTime> Updated { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> Authorized { get; set; }

        public List<Project> Projects { get; set; }
        public List<Tracker> Trackers { get; set; }
        public List<IssueStatus> Statuss { get; set; }
        public List<IssuePriority> Priorities { get; set; }
        public List<TimeEntryActivity> TimeEntryActivities { get; set; }
        public List<Query> Queries { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public Dictionary<int, List<ProjectMembership>> ProjectMemberships { get; set; }
        public Dictionary<int, List<Redmine.Net.Api.Types.Version>> ProjectVersions { get; set; }
        public MyUser MyUser { get; set; }
        /// <summary>
        /// ステータスが「有効」のユーザの一覧
        /// </summary>
        public List<MyUser> Users { get; set; }

        public MarkupLangType MarkupLang { get; set; } = MarkupLangType.Undefined;

        public RedmineSettingsModel RedmineSetting { get; set; }
        public DateTime UpdatedDate { get; set; }


        private ReactivePropertySlim<DateTime> updated = new ReactivePropertySlim<DateTime>();
        private ReactivePropertySlim<bool> authorized = new ReactivePropertySlim<bool>();

        public CacheManager()
        {
            Updated = updated.ObserveOnUIDispatcher().ToReadOnlyReactivePropertySlim();
            Authorized = authorized.ObserveOnUIDispatcher().ToReadOnlyReactivePropertySlim();
        }

        /// <summary>
        /// Updated の通知を UIThread で行うため、本メソッドを UIThread から実行すること
        /// </summary>
        public static void Init()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Cache))
            {
                try
                {
                    var result = CloneExtentions.ToObject<CacheManager>(Properties.Settings.Default.Cache);
                    Default = result;
                }
                catch
                {
                    Logger.Warn("Failed to read the json of Cache.");
                    Default = new CacheManager();
                }
            }
            else
            {
                Default = new CacheManager();
            }
        }

        public bool NeedsUpdate()
        {
            // 以下の対応で値がなかった場合、空のリストを返すようにしたためチェック追加
            // http://133.242.159.37/issues/1603
            if (Projects == null || Projects.Any(p => p.Trackers == null ||
                                                      p.EnabledModules == null ||
                                                      p.CustomFields == null ||
                                                      p.TimeEntryActivities == null))
                return true;

            if (CustomFields == null || CustomFields.Any(c => c.Trackers == null))
                return true;

            if (Trackers == null ||
                Statuss == null ||
                Priorities == null ||
                TimeEntryActivities == null ||
                Users == null ||
                Queries == null ||
                MyUser == null ||
                MarkupLang == MarkupLangType.Undefined ||
                ProjectMemberships == null)
                return true;

            return false;
        }

        public void UpdateCacheIfNeeded(RedmineManager redmine, RedmineSettingsModel settings)
        {
            this.redmine = redmine;
            this.settings = settings;

            authorized.Value = true;

            // キャッシュが存在しない場合、またはRedmine設定が変更された場合、前回接続に失敗していた場合、
            if (NeedsUpdate() || RedmineSetting == null || !RedmineSetting.Equals(settings) || (DateTime.Now - UpdatedDate).TotalDays > 10)
            {
                // 初回は同期的に更新する
                Update(redmine, settings);
            }
        }

        private RedmineManager redmine { get; set; }
        private RedmineSettingsModel settings { get; set; }
        public void ForceUpdate()
        {
            UpdateCacheIfNeeded(redmine, settings);
        }

        public void SaveCache(RedmineManager redmine, RedmineSettingsModel settings)
        {
            if (redmine == null)
                return;

            try
            {
                Update(redmine, settings, false);

                Properties.Settings.Default.Cache = Default.ToJson();
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to update cache on WindowClosed");
            }
        }

        private CancellationTokenSource cts;
        /// <summary>
        /// キャッシュと設定を更新する
        /// </summary>
        public void Update(RedmineManager redmine, RedmineSettingsModel settings, bool needsNotify = true)
        {
            try
            {
                cts?.Cancel();
                cts = new CancellationTokenSource();

                var tMyUser = Task.Run(() => redmine.GetMyUser(), cts.Token);
                var tProjects = Task.Run(() => redmine.GetProjects(), cts.Token);
                var tTrackers = Task.Run(() => redmine.GetTrackers(), cts.Token);
                var tStatuss = Task.Run(() => redmine.GetStatuss(), cts.Token);
                var tPriorities = Task.Run(() => redmine.GetPriorities(), cts.Token);
                var tTimeEntryActivities = Task.Run(() => redmine.GetTimeEntryActivities(), cts.Token);
                var tQueries = Task.Run(() => redmine.GetQueries(), cts.Token);
                var tCustomFields = Task.Run(() => redmine.CanUseAdminApiKey() ? redmine.GetCustomFields() : new List<CustomField>(), cts.Token);
                var tUsers = Task.Run(() => redmine.CanUseAdminApiKey() ? redmine.GetUsers() : new List<MyUser>(), cts.Token);
                var tMarkupLang = Task.Run(() => redmine.GetMarkupLangType(), cts.Token);

                var valueChanged = false;
                if (needsChange(valueChanged, MyUser, tMyUser.Result, out valueChanged))
                    MyUser = tMyUser.Result;
                if (needsChange(valueChanged, Projects, tProjects.Result, out valueChanged))
                    Projects = tProjects.Result;
                if (needsChange(valueChanged, Users, tUsers.Result, out valueChanged))
                    Users = tUsers.Result;

                var tProjectMemberships = Task.Run(async () =>
                {
                    var tMemberships = MyUser.Memberships.Where(m => IsMyProject(m.Project.Id)).Select(m =>
                    {
                        return Task.Run(() => (ProjectId: m.Project.Id, Memberships: redmine.GetMemberships(m.Project.Id)));
                    }).ToList();
                    var memberships = await Task.WhenAll(tMemberships);
                    var result = new Dictionary<int, List<ProjectMembership>>();
                    foreach (var p in memberships)
                    {
                        // プロジェクトの MemeberShips にはロック中のユーザも含まれるため除外する
                        result[p.ProjectId] = p.Memberships.Where(m => m.User != null && Users.Any(u => u.Id == m.User.Id)).ToList();
                    }
                    return result;
                }, cts.Token);

                var tProjectVersions = Task.Run(async () =>
                {
                    var tVersions = MyUser.Memberships.Where(m => IsMyProject(m.Project.Id)).Select(m =>
                    {
                        return Task.Run(() => (ProjectId: m.Project.Id, Versions: redmine.GetVersions(m.Project.Id)));
                    }).ToList();
                    var versions = await Task.WhenAll(tVersions);
                    return versions.ToDictionary(p => p.ProjectId, p => p.Versions);
                }, cts.Token);

                if (needsChange(valueChanged, Trackers, tTrackers.Result, out valueChanged))
                    Trackers = tTrackers.Result;
                if (needsChange(valueChanged, Statuss, tStatuss.Result, out valueChanged))
                    Statuss = tStatuss.Result;
                if (needsChange(valueChanged, Priorities, tPriorities.Result, out valueChanged))
                    Priorities = tPriorities.Result;
                if (needsChange(valueChanged, TimeEntryActivities, tTimeEntryActivities.Result, out valueChanged))
                    TimeEntryActivities = tTimeEntryActivities.Result;
                if (needsChange(valueChanged, Queries, tQueries.Result, out valueChanged))
                    Queries = tQueries.Result;
                if (needsChange(valueChanged, CustomFields, tCustomFields.Result, out valueChanged))
                    CustomFields = tCustomFields.Result;
                if (needsChange(valueChanged, ProjectMemberships, tProjectMemberships.Result, out valueChanged))
                    ProjectMemberships = tProjectMemberships.Result;
                if (needsChange(valueChanged, ProjectVersions, tProjectVersions.Result, out valueChanged))
                    ProjectVersions = tProjectVersions.Result;
                if (valueChanged || MarkupLang != tMarkupLang.Result)
                {
                    MarkupLang = tMarkupLang.Result;
                    valueChanged = true;
                }

                RedmineSetting = settings;

                if (valueChanged)
                    Logger.Info("Updating cache has completed.");
                else
                    Logger.Info("Updating cache has completed but there is no change.");

                if (needsNotify && valueChanged)
                {
                    ClearShortCache();
                    updated.Value = DateTime.Now;
                }

                UpdatedDate = DateTime.Now;
            }
            catch (Exception e)
            {
                cts.Cancel();

                if (e is AggregateException ae &&
                    ae.InnerException is RedmineApiException rae)
                {
                    if (rae.InnerException is UnauthorizedException)
                    {
                        Logger.Error(rae, "Updating cache failed by UnauthorizedException.");
                        authorized.Value = false;
                        throw new ApplicationException(Properties.Resources.msgErrUnauthorizedRedmineSettings, e);
                    }
                    else if (rae.InnerException is ForbiddenException)
                    {
                        Logger.Error(rae, $"Updating cache failed by ForbiddenException.");
                        authorized.Value = false;
                        throw new ApplicationException(string.Format(Properties.Resources.msgErrFailedToGetForbidden, rae.Message), e);
                    }

                    throw;
                }
                else if (e is RedmineLoginFailedException)
                {
                    Logger.Error(e, "Updating cache failed by Login Failure");
                    authorized.Value = false;
                    throw new ApplicationException(Properties.Resources.msgErrUnauthorizedRedmineSettings, e);
                }
                else
                {
                    throw;
                }
            }
        }

        private bool needsChange<T>(bool allreadyChanged, T oldValue, T newValue, out bool valueChanged) where T : class
        {
            if (allreadyChanged || oldValue == null || oldValue.ToJson() != newValue.ToJson())
            {
                valueChanged = true;
                return true;
            }
            else
            {
                valueChanged = false;
                return false;
            }
        }

        public bool IsActiveProject(int projectId)
        {
            var proj = Projects.FirstOrDefault(p => p.Id == projectId);
            return proj != null && proj.Status == ProjectStatus.Active;
        }

        public bool IsMyProject(int projectId)
        {
            return MyUser.Memberships.Any(m => projectId == m.Project.Id);
        }

        // 応答性向上のための短期的なキャッシュ
        [JsonIgnore]
        public ConcurrentDictionary<string, Project> ProjectsShortCache { get; set; } = new ConcurrentDictionary<string, Project>();
        [JsonIgnore]
        public ConcurrentDictionary<string, Issue> JournalIssuesShortCache { get; set; } = new ConcurrentDictionary<string, Issue>();
        [JsonIgnore]
        public ConcurrentDictionary<string, int> IssueCountShortCache { get; set; } = new ConcurrentDictionary<string, int>();
        [JsonIgnore]
        public ConcurrentDictionary<string, TimeEntry> TimeEntriesShortCache { get; set; } = new ConcurrentDictionary<string, TimeEntry>();

        public void ClearShortCache()
        {
            ProjectsShortCache.Clear();
            JournalIssuesShortCache.Clear();
            IssueCountShortCache.Clear();
            TimeEntriesShortCache.Clear();
        }

        // 設定ダイアログで最新の情報を使用するための一時的なキャッシュ
        // 以下のプロパティにアクセスする場合、事前に UpdateTemporaryCacheAsync を実行しておくこと。
        public List<Project> TmpProjects { get; set; }
        public List<Tracker> TmpTrackers { get; set; }
        public List<IssueStatus> TmpStatuss { get; set; }
        public List<TimeEntryActivity> TmpTimeEntryActivities { get; set; }
        public List<Query> TmpQueries { get; set; }
        public List<CustomField> TmpCustomFields { get; set; }
        public List<CustomField> TmpMyCustomFields { get; set; }
        public List<MyUser> TmpUsers { get; set; }
        public MyUser TmpMyUser { get; set; }
        public MarkupLangType TmpMarkupLang { get; set; }

        /// <summary>
        /// 一時的なキャッシュを Redmine から取得する。TmpXXX にアクセスする前に適切なタイミングで本メソッドを実行すること。
        /// </summary>
        public async Task UpdateTemporaryCacheAsync(RedmineManager redmine)
        {
            var tProjects = Task.Run(() => redmine.GetProjects());
            var tTrackers = Task.Run(() => redmine.GetTrackers());
            var tStatuss = Task.Run(() => redmine.GetStatuss());
            var tTimeEntryActivities = Task.Run(() => redmine.GetTimeEntryActivities());
            var tQueries = Task.Run(() => redmine.GetQueries());
            var tCustomFields = Task.Run(() => redmine.CanUseAdminApiKey() ? redmine.GetCustomFields() : new List<CustomField>());
            var tUsers = Task.Run(() => redmine.CanUseAdminApiKey() ? redmine.GetUsers() : new List<MyUser>());
            var tMyUser = Task.Run(() => redmine.GetMyUser());
            var tMarkupLang = Task.Run(() => redmine.GetMarkupLangType());

            await Task.WhenAll(tProjects, tTrackers, tStatuss, tTimeEntryActivities, tQueries, tCustomFields, tUsers, tMyUser, tMarkupLang);

            TmpProjects = tProjects.Result;
            TmpTrackers = tTrackers.Result;
            TmpStatuss = tStatuss.Result;
            TmpTimeEntryActivities = tTimeEntryActivities.Result;
            TmpQueries = tQueries.Result;
            TmpCustomFields = tCustomFields.Result;
            TmpUsers = tUsers.Result;
            TmpMyUser = tMyUser.Result;
            TmpMarkupLang = tMarkupLang.Result;

            TmpMyCustomFields = new List<CustomField>();
            if (redmine.CanUseAdminApiKey())
            {
                var enableCfIds = TmpProjects.Where(p => TmpMyUser.Memberships.Any(m => p.Name == m.Project.Name))
                    .Where(p => p.CustomFields != null)
                    .SelectMany(p => p.CustomFields.Select(a => a.Id))
                    .Distinct().ToList();
                TmpMyCustomFields = TmpCustomFields.Where(c => c.IsIssueType() && enableCfIds.Contains(c.Id)).ToList();
            }
        }
    }
}
