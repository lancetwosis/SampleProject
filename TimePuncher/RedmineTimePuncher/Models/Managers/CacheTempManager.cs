using LibRedminePower.Enums;
using LibRedminePower.Logging;
using LibRedminePower.Models;
using Microsoft.VisualBasic.ApplicationServices;
using NetOffice.VBIDEApi;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Managers
{
    public class CacheTempManager
    {
        public static CacheTempManager Default { get; set; } = new CacheTempManager();
        public ReactivePropertySlim<RedmineManager> Redmine { get; set; } = new ReactivePropertySlim<RedmineManager>();
        public ReactivePropertySlim<string> Message { get; set; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<DateTime> Updated { get; } = new ReactivePropertySlim<DateTime>();

        public ReactivePropertySlim<List<Project>> Projects { get; set; } = new ReactivePropertySlim<List<Project>>();
        public ReadOnlyReactivePropertySlim<List<MyProject>> MyProjects { get; set; } 
        public ReadOnlyReactivePropertySlim<List<MyProject>> MyProjectsWiki { get; set; }
        public ReactivePropertySlim<List<Tracker>> Trackers { get; set; } = new ReactivePropertySlim<List<Tracker>>();
        public ReadOnlyReactivePropertySlim<List<MyTracker>> MyTrackers { get; set; } 
        public ReactivePropertySlim<List<IssueStatus>> Statuss { get; set; } = new ReactivePropertySlim<List<IssueStatus>>();
        public ReactivePropertySlim<List<TimeEntryActivity>> TimeEntryActivities { get; set; } = new ReactivePropertySlim<List<TimeEntryActivity>>();
        public ReactivePropertySlim<List<Query>> Queries { get; set; } = new ReactivePropertySlim<List<Query>>();
        public ReadOnlyReactivePropertySlim<List<MyQuery>> MyQueries { get; set; } 
        public ReactivePropertySlim<List<CustomField>> CustomFields { get; set; } = new ReactivePropertySlim<List<CustomField>>();
        public ReactivePropertySlim<List<CustomField>> MyCustomFields { get; set; } = new ReactivePropertySlim<List<CustomField>>();
        public ReactivePropertySlim<List<MyUser>> MyUsers { get; set; } = new ReactivePropertySlim<List<MyUser>>();
        public ReactivePropertySlim<MyUser> MyUser { get; set; } = new ReactivePropertySlim<MyUser>();
        public ReadOnlyReactivePropertySlim<List<MyUser>> MyOtherUsers { get; set; }
        public ReactivePropertySlim<MarkupLangType> MarkupLang { get; set; } = new ReactivePropertySlim<MarkupLangType>();
        public ReactivePropertySlim<bool> CanUseAdminApiKey { get; set; } = new ReactivePropertySlim<bool>();

        public CacheTempManager()
        {
            var myProject = Projects.Where(a => a != null).Zip(MyUser.Where(a => a != null), (projs, myUser) =>
            projs.Where(p => myUser.Memberships.Any(m => p.Name == m.Project.Name)).ToList()).ToReadOnlyReactivePropertySlim();
            MyProjects = myProject.Where(a => a != null).Select(projs => projs.Select(a => new MyProject(a)).ToList()).ToReadOnlyReactivePropertySlim();
            MyProjectsWiki = myProject.Where(a => a != null).Select(myProjs => 
            myProjs.Where(p => p.EnabledModules.Any(m => m.Name == RedmineKeys.WIKI)).Select(p => new MyProject(p)).ToList()).ToReadOnlyReactivePropertySlim();
            MyTrackers = Trackers.Select(a => a?.Select(b => new MyTracker(b)).ToList()).ToReadOnlyReactivePropertySlim();
            MyQueries = Queries.Select(a => a?.Select(b => new MyQuery(b)).ToList()).ToReadOnlyReactivePropertySlim();
            MyOtherUsers = MyUsers.Where(a => a != null).Zip(MyUser.Where(a => a != null), 
                (users, my) => users.Where(a => a.Id != my.Id).ToList()).ToReadOnlyReactivePropertySlim();
        }

        private BusyNotifier isBusy = new BusyNotifier(); 
        public async Task TryConnectAsync(RedmineSettingsModel setting)
        {
            // 複数回実行された場合に、後発をスキップする。
            if (isBusy.IsBusy) return;
            using (isBusy.ProcessStart())
            {
                // Redmineが無かったり、設定が変わっていたら
                if (Redmine.Value == null || !Redmine.Value.Settings.Equals(setting))
                {
                    // 設定が検証済みならば、
                    if (setting.IsValid.Value)
                    {
                        // 接続開始する。
                        Message.Value = Properties.Resources.SettingsMsgNowConnecting;
                        try
                        {
                            // 接続する。
                            var r = new RedmineManager(setting);
                            await r.CheckConnectAsync();

                            // 更新する。
                            await updateAsync(r);

                            Message.Value = null;
                        }
                        catch (Exception ex)
                        {
                            Message.Value = ex.Message;
                            Logger.Error(ex, "Failed to create RedmineManager on SettingsViewModel.");
                        }
                    }
                    else
                    {
                        Message.Value = Properties.Resources.SettingsMsgSetRedmineSettings;
                    }
                }
            }
        }

        private async Task updateAsync(RedmineManager redmine)
        {
            // Redmineの接続先が本キャッシュと同じであれば、本キャッシュのデータを使う。
            if (redmine.Settings.Equals(CacheManager.Default.RedmineSetting))
            {
                Projects.Value = CacheManager.Default.Projects;
                Trackers.Value = CacheManager.Default.Trackers;
                Statuss.Value = CacheManager.Default.Statuss;
                TimeEntryActivities.Value = CacheManager.Default.TimeEntryActivities;
                Queries.Value = CacheManager.Default.Queries;
                CustomFields.Value = CacheManager.Default.CustomFields;
                MyUsers.Value = CacheManager.Default.Users;
                MyUser.Value = CacheManager.Default.MyUser;
                MarkupLang.Value = CacheManager.Default.MarkupLang;
            }
            else
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

                Projects.Value = tProjects.Result;
                Trackers.Value = tTrackers.Result;
                Statuss.Value = tStatuss.Result;
                TimeEntryActivities.Value = tTimeEntryActivities.Result;
                Queries.Value = tQueries.Result;
                CustomFields.Value = tCustomFields.Result;
                MyUsers.Value = tUsers.Result;
                MyUser.Value = tMyUser.Result;
                MarkupLang.Value = tMarkupLang.Result;
            }

            if (redmine.CanUseAdminApiKey())
            {
                var enableCfIds = Projects.Value.Where(p => MyUser.Value.Memberships.Any(m => p.Name == m.Project.Name))
                    .Where(p => p.CustomFields != null)
                    .SelectMany(p => p.CustomFields.Select(a => a.Id))
                    .Distinct().ToList();
                MyCustomFields.Value = CustomFields.Value.Where(c => c.IsIssueType() && enableCfIds.Contains(c.Id)).ToList();
            }
            else
            {
                MyCustomFields.Value = new List<CustomField>();
            }

            CanUseAdminApiKey.Value = redmine.CanUseAdminApiKey();
            Redmine.Value = redmine;
            Updated.Value = DateTime.Now;
        }
    }
}
