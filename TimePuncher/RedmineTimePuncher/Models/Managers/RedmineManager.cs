﻿using LibRedminePower;
using LibRedminePower.Enums;
using LibRedminePower.Exceptions;
using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using LibRedminePower.Models.Manager;
using LibRedminePower.Proxy;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Exceptions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Models.Managers
{
    public class RedmineManager : LibRedminePower.Models.Bases.ModelBase
    {
        public static int TickLength;

        public string Host => Manager.Host;

        public IRedmineManager Manager { get; set; }
        public IRedmineManager MasterManager { get; set; }
        public RedmineSettingsModel Settings { get; }
        private RedmineWebManager webManager;

        private ConcurrentDictionary<string, Issue> dicJournalIssues;
        private ConcurrentDictionary<string, int> dicIssueCount;
        private ConcurrentDictionary<string, TimeEntry> dicTimeEntries;
        private ConcurrentDictionary<string, List<ProjectMembership>> dicMemberShips;
        private ConcurrentDictionary<int, List<Redmine.Net.Api.Types.Version>> dicVersions;

        private Regex regIssuePattern = new Regex(@"#\d+", RegexOptions.Compiled);
        private Regex regIssue2Pattern = new Regex(@"issues/\d+", RegexOptions.Compiled);

        private DebugDataManager debugDataManager = new DebugDataManager();

        public RedmineManager(RedmineSettingsModel settings)
        {
            this.Settings = settings;

            ClearCash();
            var sem = new SemaphoreSlim(settings.ConcurrencyMax);
            if (settings.UseBasicAuth)
            {
                // ログインに失敗してしまう現象があるため、「Redmine.Net.Api.RedmineManager」よりも前に RedmineWebManager を生成する。
                webManager = new RedmineWebManager(settings.UrlBase, settings.UserName, settings.Password, settings.UserNameOfBasicAuth, settings.PasswordOfBasicAuth);
                Manager = LoggingAdvice<IRedmineManager>.Create(
                    new Redmine.Net.Api.RedmineManager(settings.UrlBase, settings.UserNameOfBasicAuth, settings.PasswordOfBasicAuth, settings.ApiKey),
                    () => sem.Wait(), () => sem.Release(),
                    s => { if (!s.StartsWith("get_Host")) Logger.Info(s); },
                    s => Logger.Error(s), o => o?.ToString());

                if (!string.IsNullOrEmpty(Settings.AdminApiKey))
                {
                    var sem2 = new SemaphoreSlim(Settings.ConcurrencyMax);
                    MasterManager = LoggingAdvice<IRedmineManager>.Create(
                        new Redmine.Net.Api.RedmineManager(Settings.UrlBase, Settings.UserNameOfBasicAuth, Settings.PasswordOfBasicAuth, Settings.AdminApiKey),
                        () => sem.Wait(), () => sem.Release(),
                        s => { if (!s.StartsWith("get_Host")) Logger.Info(s); },
                        s => Logger.Error(s), o => o?.ToString());
                }
            }
            else
            {
                webManager = new RedmineWebManager(settings.UrlBase, settings.UserName, settings.Password);
                Manager = LoggingAdvice<IRedmineManager>.Create(
                    new Redmine.Net.Api.RedmineManager(settings.UrlBase, settings.UserName, settings.Password),
                    () => sem.Wait(), () => sem.Release(),
                    s => { if (!s.StartsWith("get_Host")) Logger.Info(s); },
                    s => Logger.Error(s), o => o?.ToString());

                if (!string.IsNullOrEmpty(Settings.AdminApiKey))
                {
                    var sem2 = new SemaphoreSlim(Settings.ConcurrencyMax);
                    MasterManager = LoggingAdvice<IRedmineManager>.Create(
                        new Redmine.Net.Api.RedmineManager(Settings.UrlBase, Settings.UserName, Settings.Password, Settings.AdminApiKey),
                        () => sem.Wait(), () => sem.Release(),
                        s => { if (!s.StartsWith("get_Host")) Logger.Info(s); },
                        s => Logger.Error(s), o => o?.ToString());
                }
            }
        }

        public void ClearCash()
        {
            dicJournalIssues = new ConcurrentDictionary<string, Issue>();
            dicIssueCount = new ConcurrentDictionary<string, int>();
            dicTimeEntries = new ConcurrentDictionary<string, TimeEntry>();
            dicMemberShips = new ConcurrentDictionary<string, List<ProjectMembership>>();
            dicVersions = new ConcurrentDictionary<int, List<Redmine.Net.Api.Types.Version>>();
        }

        public List<Project> GetProjects() =>
            Manager.GetObjectsWithErrConv<Project>(RedmineKeys.TIME_ENTRY_ACTIVITIES, RedmineKeys.TRACKERS, RedmineKeys.ENABLED_MODULES);
        public List<Tracker> GetTrackers() =>
            Manager.GetObjectsWithErrConv<Tracker>();
        public List<IssueStatus> GetStatuss() =>
            Manager.GetObjectsWithErrConv<IssueStatus>();
        public List<IssuePriority> GetPriorities() => 
            Manager.GetObjectsWithErrConv<IssuePriority>();
        public List<TimeEntryActivity> GetTimeEntryActivities() =>
            Manager.GetObjectsWithErrConv<TimeEntryActivity>();
        public List<CustomField> GetCustomFields() => 
            getObjectsByMasterManager<CustomField>();
        public List<MyUser> GetUsers() =>
            getObjectsByMasterManager<User>().Select(u => new MyUser(u)).ToList();
        public MyUser GetMyUser() =>
            new MyUser(Manager.GetCurrentUserWithErrConv(new NameValueCollection() { { RedmineKeys.INCLUDE, RedmineKeys.MEMBERSHIPS } }));
        public List<Query> GetQueries() => 
            Manager.GetObjectsWithErrConv<Query>();
        public async Task<MarkupLangType> GetMarkupLangTypeAsync()
        {
            var issues = await Task.Run(() => Manager.GetObjectsWithErrConv<Issue>(new NameValueCollection() { { RedmineKeys.LIMIT, "1" } }));
            if (issues == null || !issues.Any())
                return MarkupLangType.Textile;
            else
                return await webManager.GetMarkupLangTypeFromTicketAsync($"{Settings.UrlBase}issues/{issues[0].Id}");
        }

        public async Task CheckConnectAsync()
        {
            try
            {
                await Task.Run(() => GetMyUser());
            }
            catch (Exception ex)
            {
                if (ex is RedmineConnectionException)
                    throw;
                else
                    throw new RedmineConnectionException(ex);
            }

            if (!CanUseAdminApiKey())
            {
                try
                {
                    await Task.Run(() => MasterManager.GetCurrentUserWithErrConv());
                }
                catch (Exception)
                {
                    throw new ApplicationException(Properties.Resources.RedmineMngMsgIncorrectAdminAPIKey);
                }
            }
        }

        private List<T> getObjectsByMasterManager<T>(NameValueCollection prms = null) where T : class, new()
        {
            if (!CanUseAdminApiKey())
                throw new ApplicationException(Properties.Resources.RedmineMngMsgAdminAPIKeyNotSet);
            return MasterManager.GetObjectsWithErrConv<T>(prms != null ? prms : new NameValueCollection());
        }

        public bool CanUseAdminApiKey() => MasterManager != null;

        public List<ProjectMembership> GetMemberships(int projectId)
        {
            return dicMemberShips.GetOrAdd(projectId.ToString(), _ => getObjectsByMasterManager<ProjectMembership>(
                new NameValueCollection() { { RedmineKeys.PROJECT_ID, projectId.ToString() } }));
        }

        public string GetTicketNo(params string[] contents)
        {
            foreach (var content in contents)
            {
                foreach (var line in content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var result = regIssuePattern.Matches(line).Cast<Match>().Select(a => a.Value).FirstOrDefault()?.Substring(1);
                    if (!string.IsNullOrEmpty(result) && getCount(result) > 0) return result;
                    if (line.Contains(Settings.UrlBase))
                    {
                        result = regIssue2Pattern.Matches(line).Cast<Match>().Select(a => a.Value).FirstOrDefault()?.Split('/').Last();
                        if (!string.IsNullOrEmpty(result) && getCount(result) > 0) return result;
                    }
                }
            }
            return null;
        }
        public string GetTicketNo(string refsKeywords, string content)
        {
            var ptrns = refsKeywords.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(a => new Regex($@"{a.Trim()} #\d+", RegexOptions.IgnoreCase));
            foreach (var line in content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach(var ptrn in ptrns)
                {
                    var result = ptrn.Matches(line).Cast<Match>().Select(a => a.Value).FirstOrDefault();
                    if (!string.IsNullOrEmpty(result))
                    {
                        var result2 = result.Substring(result.IndexOf('#') + 1);
                        if (getCount(result2) > 0)
                        {
                            return result2;
                        }                        
                    }
                }
            }
            return null;
        }

        public string GetTicketNoFromUrl(string url)
        {
            var result = regIssue2Pattern.Matches(url).Cast<Match>().Select(a => a.Value).FirstOrDefault()?.Split('/').Last();
            if (!string.IsNullOrEmpty(result)) return result;
            return null;
        }

        public List<Redmine.Net.Api.Types.Version> GetVersions(int projectId)
        {
            return dicVersions.GetOrAdd(projectId, _ => Manager.GetObjectsWithErrConv<Redmine.Net.Api.Types.Version>(projectId));
        }

        public IEnumerable<MyIssue> GetMyTickets()
        {
            return exec(nameof(GetMyTickets),
                () =>
                {
                    var results = Manager.GetObjectsWithErrConv<Issue>(new NameValueCollection { { "assigned_to_id", $"{CacheManager.Default.MyUser.Value.Id}" } });
                    return results != null ? results.Select(a => new MyIssue(a)) : Enumerable.Empty<MyIssue>();
                },
                e => Properties.Resources.RedmineMngMsgFailedToGetMyTicket);
        }

        public IEnumerable<MyIssue> GetTicketsByIds(List<string> ids)
        {
            return exec(nameof(GetTicketsByIds),
                () =>
                {
                    return !ids.Any() ?
                           new List<MyIssue>() :
                           Manager.GetObjectsWithErrConv<Issue>(RedmineKeys.ISSUE_ID, ids).Select(i => new MyIssue(i));
                },
                e => $"{Properties.Resources.RedmineMngMsgFailedToGetTicket} (Id={string.Join(", ", ids)} Message={e.Message})");
        }

        public MyIssue GetTicketsById(string id)
        {
            return exec(nameof(GetTicketsById),
                () => new MyIssue(Manager.GetObjectWithErrConv<Issue>(id, new NameValueCollection())),
                e => $"{Properties.Resources.RedmineMngMsgFailedToGetTicket} (Id={id} Message={e.Message})");
        }

        public IEnumerable<MyIssue> GetTicketsByQuery(MyQuery query)
        {
            return exec(nameof(GetTicketsByQuery),
                () =>
                {
                    var results = Manager.GetObjectsWithErrConv<Issue>(new NameValueCollection
                    {
                        { RedmineKeys.PROJECT_ID, $"{query.ProjectId}" },
                        { RedmineKeys.QUERY_ID, $"{query.Id}" }
                    });
                    return results != null ? results.Select(a => new MyIssue(a)) : Enumerable.Empty<MyIssue>();
                },
                e => string.Format(Properties.Resources.RedmineMngMsgFailedToGetTicketsByQuery, query.Name));
        }

        public List<TimeEntry> GetTimeEntries(NameValueCollection parameters)
        {
            return exec(nameof(GetTimeEntries),
                () =>
                {
                    return getObjectsWithPaging<TimeEntry>(parameters);
                },
                e => "TimeEntry の取得に失敗しました。");
        }

        private List<T> getObjectsWithPaging<T>(NameValueCollection parameters) where T : class, new()
        {
            var limit = 80;
            parameters.Set(RedmineKeys.LIMIT, limit.ToString());
            parameters.Set(RedmineKeys.OFFSET, "0");

            var items = Manager.GetObjectsWithErrConv<T>(parameters);
            if (items == null)
                return new List<T>();
            else if (items.Count < limit)
                return items;

            var allItems = new List<T>(items);
            var offset = limit;
            while (true)
            {
                parameters.Set(RedmineKeys.OFFSET, offset.ToString());
                items = Manager.GetObjectsWithErrConv<T>(parameters);
                if (items == null)
                    break;

                allItems.AddRange(items);
                if (items.Count < limit)
                    break;
                else
                    offset += limit;
            }

            return allItems;
        }

        public Issue GetIssue(string id)
        {
            return exec(nameof(GetTicketsById),
                () => Manager.GetObjectWithErrConv<Issue>(id, new NameValueCollection()
                    { { RedmineKeys.INCLUDE, RedmineKeys.CHILDREN } }),
                e => $"{Properties.Resources.RedmineMngMsgFailedToGetTicket} (Id={id} Message={e.Message})");
        }

        public List<Issue> GetIssues(List<string> ids)
        {
            return exec(nameof(GetTicketsById),
                () => Manager.GetObjectsWithErrConv<Issue>(RedmineKeys.ISSUE_ID, ids, new NameValueCollection() { { RedmineKeys.STATUS_ID, "*" } }),
                e => $"{Properties.Resources.RedmineMngMsgFailedToGetTicket} (Id={string.Join(", ", ids)} Message={e.Message})");
        }

        public List<Issue> GetChildIssues(string id)
        {
            return exec(nameof(GetChildIssues),
                () =>
                {
                    var prms = new NameValueCollection()
                    {
                        { RedmineKeys.PARENT_ID, $"~{id}" },
                        { RedmineKeys.STATUS_ID, "*" },
                    };

                    return getObjectsWithPaging<Issue>(prms);
                },
                e => $"#{id}の子チケットの取得に失敗しました。");
        }

        /// <summary>
        /// 指定されたチケットの最上位の親チケットのチケット番号を返す。自身が最上位の場合、null を返す。
        /// </summary>
        public async Task<int?> GetTopIssueIdAsync(string id)
        {
            var issues = await webManager.GetParentIssuesAsync($"{Host}issues/{id}");
            if (issues.Any())
                return issues[0].Id;
            else
                return null;
        }

        /// <summary>
        /// 取得に失敗してもアプリを終了させたくない場合に使用する
        /// </summary>
        private T exec<T>(string name, Func<T> func, Func<Exception, string> errmsgCreator)
        {
            using (Logger.CreateProcess<RedmineManager>(name))
            {
                try
                {
                    return func.Invoke();
                }
                catch (Exception e)
                {
                    throw new ApplicationException(errmsgCreator.Invoke(e), e);
                }
            }
        }

        public string GetTicketUrl(string no) => Settings.UrlBase + $"issues/{no}";

        public MyIssue GetTicketIncludeJournal(string id, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(id))
            {
                error = Properties.Resources.RedmineMngMsgIssueIdNotSet;
                return null;
            }

            try
            {
                var issue = GetIssueIncludeJournal(id);
                if (issue == null) return null;
                return new MyIssue(issue);
            }
            catch(Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// コメントに引数の文字列が含まれる自分の TimeEntry を取得する
        /// </summary>
        public TimeEntry GetTimeEntry(string subject)
        {
            return dicTimeEntries.GetOrAdd(subject, _ =>
            {
                var parameters = new NameValueCollection()
                {
                    // 「~」を追加することで文字列が正規表現として扱われる
                    { RedmineKeys.COMMENTS, $"~{System.Web.HttpUtility.UrlEncode(subject)}" },
                    { RedmineKeys.LIMIT, "1" },
                    { RedmineKeys.USER_ID, $"{CacheManager.Default.MyUser.Value.Id}" },
                };
                var entries = Manager.GetObjectsWithErrConv<TimeEntry>(parameters);
                return entries != null ? entries.FirstOrDefault() : null;
            });
        }

        /// <summary>
        /// memberResources で指定されたユーザ＋自分自身の TimeEntry を取得する
        /// </summary>
        public List<MyTimeEntry> GetTimeEntries(List<MemberResource> memberResources, DateTime start, DateTime end, out List<int> errorIds)
        {
            var userIds = new List<string>();
            userIds.Add(CacheManager.Default.MyUser.Value.Id.ToString());
            userIds.AddRange(memberResources.Select(a => a.ResourceName));
            return GetTimeEntries(userIds, start, end, out errorIds);
        }

        /// <summary>
        /// 自分自身の TimeEntry を取得する
        /// </summary>
        public List<MyTimeEntry> GetMyTimeEntries(DateTime start, DateTime end, out List<int> errorIds)
        {
            return GetTimeEntries(new List<string>() { CacheManager.Default.MyUser.Value.Id.ToString() }, start, end, out errorIds);
        }

        public List<MyTimeEntry> GetTimeEntries(List<string> userIds, DateTime start, DateTime end, out List<int> errorIds)
        {
            var allEntries = new List<MyTimeEntry>();
            errorIds = new List<int>();
            var queryParameters = new NameValueCollection {
                { RedmineKeys.FROM, start.ToString("yyyy-MM-dd") },
                { RedmineKeys.TO, end.ToString("yyyy-MM-dd") },
                { RedmineKeys.USER_ID, string.Join("|", userIds.ToArray()) },
            };
            var entries = Manager.GetObjectsWithErrConv<TimeEntry>(queryParameters);
            if (entries != null)
            {
                // 客先にて GetTimeEntries の Where にてヌルポが発生するという報告があった。よって null チェックを追加する
                foreach (var entry in entries.Where(a => a != null && a.Comments != null && a.Comments.StartsWith("{")))
                {
                    try
                    {
                        allEntries.Add(new MyTimeEntry(entry));
                    }
                    catch (Exception)
                    {
                        errorIds.Add(entry.Id);
                    }
                }
            }
            // 客先にて GetTimeEntries の Where にてヌルポが発生するという報告があった。よって null チェックを追加する
            allEntries = allEntries.Where(a => a != null && start <= a.Start && a.End <= end).ToList();
            return allEntries;
        }

        public List<MyAppointment> GetEntryApos(Resource myResource, List<MemberResource> memberResources, DateTime start, DateTime end, out List<int> errorIds)
        {
            var result = new List<MyAppointment>();

            // Redmineから該当日時に登録された予定を全て取得する。
            var allEntries = GetTimeEntries(memberResources, start, end, out errorIds);

            // 取得した予定から、自分の作業実績、他ユーザーの作業実績、他ユーザーから招待された予定を作成する。
            foreach (var entry in allEntries.Where(a => a.Entry.Hours > 0))
            {
                var res =
                    (entry.UserId == CacheManager.Default.MyUser.Value.Id) ? myResource :
                    memberResources.FirstOrDefault(a => a.ResourceName == entry.UserId.ToString());
                MyAppointment apo = null;
                if (res != null && entry.Entry.Issue != null)
                {
                    apo = new MyAppointment(res, entry);
                    result.Add(apo);
                }

                if (entry.ToMembers.Any())
                {
                    var fromEntries = allEntries.Where(a => a.FromId == entry.Entry.Id).ToList();
                    foreach (var memberId in entry.ToMembers.Indexed())
                    {
                        if (!fromEntries.Any(a => a.UserId == memberId.v))
                        {
                            var res2 = (memberId.v == CacheManager.Default.MyUser.Value.Id) ? myResource :
                                memberResources.FirstOrDefault(a => a.ResourceName == memberId.v.ToString());
                            if (res2 != null)
                            {
                                var addApo = new MyAppointment(res2, entry, apo);
                                if (entry.ToMemberCategoryIds.Any())
                                {
                                    var categoryId = entry.ToMemberCategoryIds[memberId.i];
                                    var cat = addApo.ProjectCategories.Value.FirstOrDefault(a => a.Id == categoryId);
                                    if (cat != null)
                                        addApo.Category = cat;
                                }
                                result.Add(addApo);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public void SetEntryApos(SettingsModel settings, List<MyAppointment> apos, out List<(MyAppointment, Exception)> failList)
        {
            failList = new List<(MyAppointment, Exception)>();

            // 更新作業
            foreach(var a in apos.Where(a => a.TimeEntryId > 0).ToList())
            {
                try
                {
                    var entry = new MyTimeEntry(a);
                    if (settings.Calendar.IsWorkingDay(a.Start.Date))
                    {
                        var devided = settings.Schedule.DevideIfNeeded(entry);
                        Manager.UpdateObjectWithErrConv(a.TimeEntryId.ToString(), devided.First().Entry);
                        if (devided.Count > 1)
                        {
                            foreach (var e in devided.Skip(1))
                            {
                                Manager.CreateObjectWithErrConv(e.Entry);
                            }
                        }
                    }
                    else
                    {
                        entry.Type = Enums.TimeEntryType.OverTime;
                        Manager.UpdateObjectWithErrConv(a.TimeEntryId.ToString(), entry.Entry);
                    }
                }
                catch (RedmineApiException re) when (re.InnerException.GetType() == typeof(Redmine.Net.Api.Exceptions.NotFoundException))
                {
                    // TimeEntryが予期せず削除されてしまった場合は、新規追加すれば、登録できるのでそこにトライできるようにする。
                    a.TimeEntryId = 0;
                }
                catch (RedmineApiException re) when (re.InnerException.Message.StartsWith("Invalid or missing attribute parameters"))
                {
                    // 以下のエラーなので、登録できなかったとしても、エラーは無視する。
                    // ・更新しようとしたチケットが削除されていた場合のエラー
                }
                catch (Exception ex)
                {
                    // 上記以外は、失敗リストに登録する。
                    failList.Add((a, ex));
                }
            }

            // 新規追加
            foreach (var a in apos.Where(a => a.TimeEntryId <= 0).ToList())
            {
                try
                {
                    var entry = new MyTimeEntry(a);
                    if (settings.Calendar.IsWorkingDay(a.Start.Date))
                    {
                        var devided = settings.Schedule.DevideIfNeeded(entry);
                        foreach (var e in devided)
                        {
                            Manager.CreateObjectWithErrConv(e.Entry);
                        }
                    }
                    else
                    {
                        entry.Type = Enums.TimeEntryType.OverTime;
                        Manager.CreateObjectWithErrConv(entry.Entry);
                    }
                }
                catch (RedmineApiException re) when (re.InnerException.Message.StartsWith("Invalid or missing attribute parameters"))
                {
                    // 以下のエラーなので、登録できなかったとしても、エラーは無視する。
                    // ・更新しようとしたチケットが削除されていた場合のエラー
                }
                catch (Exception ex)
                {
                    // 上記以外は、失敗リストに登録する。
                    failList.Add((a, ex));
                }
            }
        }

        public void DelEntryApos(List<MyAppointment> apos, out List<(MyAppointment, Exception)> failList)
        {
            failList = new List<(MyAppointment, Exception)>();

            var myApos = apos.Where(a => a.TimeEntryId > 0).ToList();

            // メンバーから招待された予定を削除する場合は、否決した記録を残すために、1秒の予定を入れる。
            var memApos = myApos.Where(a => a.ApoType == Enums.AppointmentType.RedmineTimeEntryMember).ToList();
            foreach(var apo in memApos)
            {
                var entry = new MyTimeEntry(apo);
                entry.End = entry.Start.AddSeconds(1);
                entry.FromId = apo.TimeEntryId;
                try
                {
                    Manager.CreateObjectWithErrConv(entry.Entry);
                }
                catch (RedmineApiException re) when (re.InnerException.Message.StartsWith("Invalid or missing attribute parameters"))
                {
                    // 以下のエラーなので、登録できなかったとしても、エラーは無視する。
                    // ・更新しようとしたチケットが削除されていた場合のエラー
                }
                catch (Exception ex)
                {
                    // 上記以外は、失敗リストに登録する。
                    failList.Add((apo, ex));
                }
            }

            // 自分の予定だったら、削除する。
            foreach(var apo in myApos.Where(a => a.ApoType != Enums.AppointmentType.RedmineTimeEntryMember).ToList())
            {
                try
                {
                    Manager.DeleteObjectWithErrConv<TimeEntry>(apo.TimeEntryId.ToString(), new NameValueCollection());
                }
                catch(RedmineApiException re) when (re.InnerException.GetType() == typeof(Redmine.Net.Api.Exceptions.NotFoundException))
                {
                    // 既に削除されていた場合の例外は無視する。
                }
                catch (Exception ex)
                {
                    // 上記以外は、失敗リストに登録する。
                    failList.Add((apo, ex));
                }
            }
        }

        public async Task<List<MyAppointment>> GetActivityAposAsync(CancellationToken token, Resource resource, DateTime start, DateTime end)
        {
            if (debugDataManager.IsExist) return debugDataManager.GetData(resource, start, end, Enums.AppointmentType.RedmineActivity);

            // WEBから活動を取得する。
            var items = await webManager.GetActivitiesAsync(token, CacheManager.Default.MyUser.Value.Id, start, end);
            // WEB活動からそのときのチケットを復元する。
            var issueItems = items.Where(a => a.IssueId != null).ToList();
            var tasks = issueItems.Select(a => a.ToIssueAtTheTimeAsync(this)).ToList();
            await Task.Run(() => Task.WhenAll(tasks), token);

            var result1 =  await Task.Run(() => tasks.Indexed().Select(a => 
            {
                var issue = a.v.Result;
                var activity = issueItems[a.i];
                return new MyAppointment(resource,
                    Enums.AppointmentType.RedmineActivity,
                    issue.Item1.ToString(issue.Item2),
                    activity.Description,
                    activity.Date.AddMinutes((-1 * TickLength)),
                    activity.Date,
                    activity.IssueId, issue.Item1);
            }).ToList(), token);

            var result2 = items.Where(a => a.IssueId == null)
                .Select(a => new MyAppointment(resource,
                Enums.AppointmentType.RedmineActivity,
                a.Subject,
                "",
                a.Date.AddMinutes((-1 * TickLength)),
                a.Date,
                a.IssueId)).ToList();

            return result1.Concat(result2).ToList();
        }

        public Issue GetIssueIncludeJournal(string ticketNo, bool reload = false)
        {
            if (reload)
                dicJournalIssues.TryRemove(ticketNo, out _);

            return dicJournalIssues.GetOrAdd(ticketNo, _ => getIssueJournalApi(ticketNo));
        }

        private int getCount(string ticketNo)
        {
            return dicIssueCount.GetOrAdd(ticketNo, 
                _ => Manager.Count<Issue>(new NameValueCollection { { RedmineKeys.ISSUE_ID, ticketNo } }));
        }

        private Issue getIssueJournalApi(string ticketNo)
        {
            try
            {
                return Manager.GetObjectWithErrConv<Issue>(ticketNo, new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS } });
            }
            catch (Exception)
            {
                // 見つからなかった場合は、NULL応答をする。
                // これ以外の例外の場合は、集約例外に通知する
                return null;
            }
        }

        public MyIssue RestoreJournals(Issue issue, DateTime target)
        {
            var result = new MyIssue(issue);
            if (issue.Journals == null)
                return result;

            var dic = new Dictionary<DateTime, MyIssue>();
            dic.Add(DateTime.MaxValue, result);

            foreach (var journal in issue.Journals.Reverse())
            {
                if (journal.Details == null) continue;

                var targetDetails = journal.Details.Where(a => a.Property == "attr" &&
                    (a.Name == "status_id" || a.Name == "assigned_to_id"
                    //a.Name == "tracker_id" || a.Name == "subject" ||
                    ));
                if (targetDetails.Any())
                {
                    result = result.Clone();
                    dic.Add(journal.CreatedOn.Value, result);
                    foreach (var detail in targetDetails)
                    {
                        switch (detail.Name)
                        {
                            case "status_id":
                                result.Status.Id = int.Parse(detail.OldValue);
                                break;
                            case "assigned_to_id":
                                if (detail.OldValue == null)
                                    result.AssignedTo.Id = 0;
                                else
                                    result.AssignedTo.Id = int.Parse(detail.OldValue);
                                break;
                            //case "tracker_id":
                            //    result.Tracker.Id = int.Parse(detail.OldValue);
                            //    break;
                            //case "subject":
                            //    result.Subject = detail.OldValue;
                            //    break;
                            default:
                                break;
                        }
                    }
                }
            }

            var temp = dic.Pairs().FirstOrDefault(a => a.Last().Key < target && target <= a.First().Key);
            if (temp != null) return temp.First().Value;

            // 該当の変更が見つからないのであれば、最新版を返す。
            if (dic.Last().Key > target)
                return dic.Last().Value;
            else
                return dic.First().Value;
        }

        public List<MyProject> GetMyProjectsOnlyWikiEnabled()
        {
            var allProjects = CacheManager.Default.Projects.Value;
            var myProjects = allProjects.Where(p => CacheManager.Default.MyUser.Value.Memberships.Any(m => p.Id == m.Project.Id)).ToList();
            return myProjects.Where(p => p.EnabledModules.Any(m => m.Name == RedmineKeys.WIKI)).Select(p => new MyProject(p)).ToList();
        }

        public List<MyWikiPageItem> GetAllWikiPages(string projIdentifier)
        {
            return Manager.GetAllWikiPagesWithErrConv(projIdentifier).Select(w => new MyWikiPageItem(Settings.UrlBase, projIdentifier, w)).ToList();
        }

        public MyWikiPage GetWikiPage(string projectId, string title, int? version = null)
        {
            return new MyWikiPage(Settings.UrlBase, projectId, Manager.GetWikiPage(projectId, null, title, version.HasValue ? (uint)version.Value : 0));
        }

        public Issue CreateTicket(Issue issue)
        {
            return Manager.CreateObjectWithErrConv<Issue>(issue);
        }

        public void UpdateTicket(Issue issue)
        {
            Manager.UpdateObjectWithErrConv(issue.Id.ToString(), issue);
        }


        public string CreateShowAllPointIssues(Issue parent, int trackerId)
        {
            var proj = CacheManager.Default.Projects.Value.Single(a => a.Id == parent.Project.Id);
            var url = $"{Settings.UrlBase}/projects/{proj.Identifier}/issues?";
            url += $"utf8=%E2%9C%93&set_filter=1&sort=id:desc";
            url += $"&f[]=parent_id&op[parent_id]=~&v[parent_id][]={parent.Id}";
            url += $"&f[]=tracker_id&op[tracker_id]==&v[tracker_id][]={trackerId}";

            return url;
        }

        public string CreatePointIssueURL(Issue parent, int trackerId, string detectionProcess, string saveReviewer)
        {
            var proj = CacheManager.Default.Projects.Value.Single(a => a.Id == parent.Project.Id);
            var url = $"{Settings.UrlBase}/projects/{proj.Identifier}/issues/new?";
            url += $"issue[tracker_id]={trackerId}";
            url += $"&issue[parent_issue_id]={parent.Id}";
            url += $"&issue[category_id]={parent.Category?.Id}";
            url += $"&issue[fixed_version_id]={parent.FixedVersion?.Id}";
            url += $"&issue[assigned_to_id]={parent.AssignedTo.Id}";
            url += $"&issue[start_date]={parent.StartDate?.ToString("yyyyMMdd")}";
            url += $"&issue[due_date]={parent.DueDate?.ToString("yyyyMMdd")}";
            if (detectionProcess != null)
                url += $"&{detectionProcess}";
            if (saveReviewer != null)
                url += $"&{saveReviewer}";

            return url;
        }
    }
}
