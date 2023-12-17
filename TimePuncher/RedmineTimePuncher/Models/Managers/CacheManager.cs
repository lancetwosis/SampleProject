using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
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

namespace RedmineTimePuncher.Models.Managers
{
    public class CacheManager : ICacheManager
    {
        [JsonIgnore]
        public static CacheManager Default { get; } = read();
        [JsonIgnore]
        public RedmineManager Redmine { get; set; }
        public RedmineSettingsModel RedmineSetting { get; set; }

        public ReactiveProperty<List<Project>> Projects { get; set; } = new ReactiveProperty<List<Project>>();
        public ReactiveProperty<List<Tracker>> Trackers { get; set; } = new ReactiveProperty<List<Tracker>>();
        public ReactiveProperty<List<IssueStatus>> Statuss { get; set; } = new ReactiveProperty<List<IssueStatus>>();
        public ReactiveProperty<List<IssuePriority>> Priorities { get; set; } = new ReactiveProperty<List<IssuePriority>>();
        public ReactiveProperty<List<TimeEntryActivity>> TimeEntryActivities { get; set; } = new ReactiveProperty<List<TimeEntryActivity>>();
        public ReactiveProperty<List<CustomField>> CustomFields { get; set; } = new ReactiveProperty<List<CustomField>>();
        public ReactiveProperty<List<MyUser>> Users { get; set; } = new ReactiveProperty<List<MyUser>>();
        public ReactiveProperty<MyUser> MyUser { get; set; } = new ReactiveProperty<MyUser>();
        public ReactiveProperty<List<Query>> Queries { get; set; } = new ReactiveProperty<List<Query>>();
        public ReactiveProperty<MarkupLangType> MarkupLang { get; set; } = new ReactiveProperty<MarkupLangType>();

        public CacheManager()
        { }

        private static CacheManager read()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Cache))
            {
                try
                {
                    return CloneExtentions.ToObject<CacheManager>(Properties.Settings.Default.Cache);
                }
                catch { }
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
                (Redmine.CanUseAdminApiKey() && CustomFields.Value != null) &&
                Users.Value != null &&
                (Redmine.CanUseAdminApiKey() && MyUser.Value != null) &&
                Queries.Value != null &&
                MarkupLang.Value != MarkupLangType.None
                ;
        }

        public async Task UpdateAsync()
        {
            var tProjects = Task.Run(() => Redmine.GetProjects());
            var tTrackers = Task.Run(() => Redmine.GetTrackers());
            var tStatuss = Task.Run(() => Redmine.GetStatuss());
            var tPriorities = Task.Run(() => Redmine.GetPriorities());
            var tTimeEntryActivities = Task.Run(() => Redmine.GetTimeEntryActivities());
            var tCustomFields = Task.Run(() => Redmine.GetCustomFields());
            var tUsers = Task.Run(() => Redmine.GetUsers());
            var tMyUser = Task.Run(() => Redmine.GetMyUser());
            var tQueries = Task.Run(() => Redmine.GetQueries());
            var tMarkupLang = Redmine.GetMarkupLangTypeAsync();

            await Task.WhenAll(
                tProjects,
                tTrackers,
                tStatuss,
                tPriorities,
                tTimeEntryActivities,
                tCustomFields,
                tUsers,
                tMyUser,
                tQueries,
                tMarkupLang
                );

            Projects.Value = tProjects.Result;
            Trackers.Value = tTrackers.Result;
            Statuss.Value = tStatuss.Result;
            Priorities.Value = tPriorities.Result;
            TimeEntryActivities.Value = tTimeEntryActivities.Result;
            CustomFields.Value = tCustomFields.Result;
            Users.Value = tUsers.Result;
            MyUser.Value = tMyUser.Result;
            Queries.Value = tQueries.Result;
            MarkupLang.Value = tMarkupLang.Result;

            RedmineSetting = Redmine.Settings;
        }

        public static void Save()
        {
            Properties.Settings.Default.Cache = Default.ToJson();
        }
    }
}
