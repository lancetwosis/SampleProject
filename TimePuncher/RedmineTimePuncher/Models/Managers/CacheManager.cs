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

namespace RedmineTimePuncher.Models.Managers
{
    public class CacheManager : ICacheManager
    {
        [JsonIgnore]
        public static CacheManager Default { get; } = read();

        // ReactivePropertySlim だと Json からのデシリアライズが出来なかったため無印の RP を使用する
        public ReactiveProperty<List<Project>> Projects { get; set; } = new ReactiveProperty<List<Project>>();
        public ReactiveProperty<List<Tracker>> Trackers { get; set; } = new ReactiveProperty<List<Tracker>>();
        public ReactiveProperty<List<IssueStatus>> Statuss { get; set; } = new ReactiveProperty<List<IssueStatus>>();
        public ReactiveProperty<List<IssuePriority>> Priorities { get; set; } = new ReactiveProperty<List<IssuePriority>>();
        public ReactiveProperty<List<TimeEntryActivity>> TimeEntryActivities { get; set; } = new ReactiveProperty<List<TimeEntryActivity>>();
        public ReactiveProperty<List<Query>> Queries { get; set; } = new ReactiveProperty<List<Query>>();
        public ReactiveProperty<List<CustomField>> CustomFields { get; set; } = new ReactiveProperty<List<CustomField>>();
        public ReactiveProperty<Dictionary<int, List<ProjectMembership>>> ProjectMemberships { get; set; } = new ReactiveProperty<Dictionary<int, List<ProjectMembership>>>();

        public ReactiveProperty<MyUser> MyUser { get; set; } = new ReactiveProperty<MyUser>();
        public ReactiveProperty<List<MyUser>> Users { get; set; } = new ReactiveProperty<List<MyUser>>();

        public ReactiveProperty<MarkupLangType> MarkupLang { get; set; } = new ReactiveProperty<MarkupLangType>(MarkupLangType.Undefined);

        public RedmineSettingsModel RedmineSetting { get; set; }

        public CacheManager()
        { }

        private static CacheManager read()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Cache))
            {
                try
                {
                    var result = CloneExtentions.ToObject<CacheManager>(Properties.Settings.Default.Cache);
                    result.RedmineSetting.LoadProperties();

                    return result;
                }
                catch
                {
                    Logger.Warn("Failed to read the json of Cache.");
                }
            }
            return new CacheManager();
        }

        public bool IsExist()
        {
            return
                Projects.Value != null &&
                Trackers.Value != null &&
                Statuss.Value != null &&
                Priorities.Value != null &&
                TimeEntryActivities.Value != null &&
                CustomFields.Value != null &&
                Users.Value != null &&
                Queries.Value != null &&
                MyUser.Value != null &&
                MarkupLang.Value != MarkupLangType.Undefined &&
                ProjectMemberships.Value != null;
        }

        public void UpdateCacheIfNeeded(RedmineManager redmine, RedmineSettingsModel settings)
        {
            // キャッシュが存在しない場合、またはRedmine設定が変更された場合、更新を行う
            if (!Default.IsExist() ||
                Default.RedmineSetting == null || !Default.RedmineSetting.Equals(settings))
            {
                Update(redmine, settings);
            }
        }

        public void SaveCache(RedmineManager redmine, RedmineSettingsModel settings)
        {
            if (redmine == null)
                return;

            Update(redmine, settings);

            Properties.Settings.Default.Cache = Default.ToJson();
            Properties.Settings.Default.Save();
        }

        public bool IsActiveProject(int projectId)
        {
            var proj = Projects.Value.FirstOrDefault(p => p.Id == projectId);
            return proj != null && proj.Status == ProjectStatus.Active;
        }

        /// <summary>
        /// キャッシュと設定を更新する
        /// </summary>
        public void Update(RedmineManager redmine, RedmineSettingsModel settings)
        {
            // TODO: キャッシュの更新処理に失敗した場合の例外処理を検討すること

            var tMyUser = Task.Run(() => redmine.GetMyUser());
            var tProjects = Task.Run(() => redmine.GetProjects());
            var tTrackers = Task.Run(() => redmine.GetTrackers());
            var tStatuss = Task.Run(() => redmine.GetStatuss());
            var tPriorities = Task.Run(() => redmine.GetPriorities());
            var tTimeEntryActivities = Task.Run(() => redmine.GetTimeEntryActivities());
            var tQueries = Task.Run(() => redmine.GetQueries());
            var tCustomFields = Task.Run(() => redmine.CanUseAdminApiKey() ? redmine.GetCustomFields() : new List<CustomField>());
            var tUsers = Task.Run(() => redmine.CanUseAdminApiKey() ? redmine.GetUsers() : new List<MyUser>());
            var tMarkupLang = Task.Run(() => redmine.GetMarkupLangType());

            MyUser.Value = tMyUser.Result;
            Projects.Value = tProjects.Result;
            Users.Value = tUsers.Result;

            var tProjectMemberships = Task.Run(() =>
            {
                var result = new Dictionary<int, List<ProjectMembership>>();
                foreach (var m in MyUser.Value.Memberships)
                {
                    if (Projects.Value.Any(p => p.Id == m.Project.Id))
                    {
                        result[m.Project.Id] = redmine.GetMemberships(m.Project.Id);
                    }
                }
                return result;
            });

            Trackers.Value = tTrackers.Result;
            Statuss.Value = tStatuss.Result;
            Priorities.Value = tPriorities.Result;
            TimeEntryActivities.Value = tTimeEntryActivities.Result;
            Queries.Value = tQueries.Result;
            CustomFields.Value = tCustomFields.Result;
            ProjectMemberships.Value = tProjectMemberships.Result;
            MarkupLang.Value = tMarkupLang.Result;

            RedmineSetting = settings;
        }

        private List<Project> tmpProjects;
        private List<Tracker> tmpTrackers;
        private List<IssueStatus> tmpStatuss;
        private List<TimeEntryActivity> tmpTimeEntryActivities;
        private List<Query> tmpQueries;
        private List<CustomField> tmpCustomFields;
        private List<MyUser> tmpUsers;
        private MyUser tmpMyUser;
        private MarkupLangType tmpMarkupLang;
        /// <summary>
        /// 一時的なキャッシュを Redmine から取得する。GetTemporaryXXX を実行する前に適切なタイミングで本メソッドを実行すること。
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

            tmpProjects = tProjects.Result;
            tmpTrackers = tTrackers.Result;
            tmpStatuss = tStatuss.Result;
            tmpTimeEntryActivities = tTimeEntryActivities.Result;
            tmpQueries = tQueries.Result;
            tmpCustomFields = tCustomFields.Result;
            tmpUsers = tUsers.Result;
            tmpMyUser = tMyUser.Result;
            tmpMarkupLang = tMarkupLang.Result;
        }

        /// <summary>
        /// Projects の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<Project> GetTemporaryProjects() => tmpProjects;

        /// <summary>
        /// Trackers の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<Tracker> GetTemporaryTrackers() => tmpTrackers;

        /// <summary>
        /// Statuss の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<IssueStatus> GetTemporaryStatuss() => tmpStatuss;

        /// <summary>
        /// TimeEntryActivities の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<TimeEntryActivity> GetTemporaryTimeEntryActivities() => tmpTimeEntryActivities;

        /// <summary>
        /// Queries の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<Query> GetTemporaryQueries() => tmpQueries;

        /// <summary>
        /// CustomFields の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<CustomField> GetTemporaryCustomFields() => tmpCustomFields;

        /// <summary>
        /// Users の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public List<MyUser> GetTemporaryUsers() => tmpUsers;

        /// <summary>
        /// MyUser の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public MyUser GetTemporaryMyUser() => tmpMyUser;

        /// <summary>
        /// MarkupLang の一時的なキャッシュを返す。実行前に必ず UpdateTemporaryCacheAsync を実行しておくこと。
        /// </summary>
        public MarkupLangType GetTemporaryMarkupLang() => tmpMarkupLang;
    }
}
