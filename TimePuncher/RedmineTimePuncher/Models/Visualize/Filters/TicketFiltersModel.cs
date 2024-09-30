using LibRedminePower.Extentions;
using LibRedminePower.Models;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize.Filters
{
    public class TicketFiltersModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public bool IsExpanded { get; set; } = true;

        public bool SpecifyParentIssue { get; set; } = true;
        public string ParentIssueId { get; set; }

        public bool SpecifyPeriod { get; set; }
        public PeriodType PeriodMode { get; set; }
        public DateTime Start { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime End { get; set; } = DateTime.Today;

        public bool SpecifyUsers { get; set; }
        public ObservableCollection<MyUser> Users { get; set; } = new ObservableCollection<MyUser>();

        public bool SpecifyProjects { get; set; }
        public ObservableCollection<MyProject> Projects { get; set; } = new ObservableCollection<MyProject>();

        public TicketFiltersModel()
        {
        }

        public List<TimeEntry> GetTimeEntries(ScheduleSettingsModel settings, Managers.RedmineManager redmine)
        {
            var prms = new NameValueCollection();
            prms.Add(RedmineKeys.COMMENTS, "~{");

            if (SpecifyPeriod)
            {
                var start = GetStart(DateTime.Today) + settings.DayStartTime;
                var end = GetEnd(DateTime.Today).AddDays(1).Date + settings.DayStartTime;

                prms.Add(RedmineKeys.FROM, start.ToString("yyyy-MM-dd"));
                prms.Add(RedmineKeys.TO, end.ToString("yyyy-MM-dd"));
            }

            List<TimeEntry> results = null;
            if (SpecifyParentIssue)
            {
                // ~ をつけることで子チケットの TimeEntry も含めて再帰的に取得
                prms.Add(RedmineKeys.ISSUE_ID, $"~{ParentIssueId}");
                results = redmine.GetTimeEntries(prms);
            }
            else if (SpecifyProjects)
            {
                results = getTimeEntries(redmine, prms, Projects.Select(p => p.Id).ToList());
            }
            else
            {
                // 自分にアサインされているプロジェクトを対象とする
                results = getTimeEntries(redmine, prms, CacheManager.Default.MyUser.Memberships.Select(m => m.Project.Id).ToList());
            }

            if (results == null || results.Count == 0)
                return new List<TimeEntry>();

            if (SpecifyUsers)
                return results.Where(t => Users.Any(u => u.Id == t.User.Id)).ToList();
            else
                return results.ToList();
        }

        private List<TimeEntry> getTimeEntries(Managers.RedmineManager redmine, NameValueCollection prms, List<int> projectIds)
        {
            var results = new List<TimeEntry>();
            foreach (var id in projectIds)
            {
                prms.Set(RedmineKeys.PROJECT_ID, id.ToString());
                var r = redmine.GetTimeEntries(prms);
                if (r != null && r.Count != 0)
                    results.AddRange(r);
            }

            // PROJECT_ID 指定で取得すると子プロジェクトの作業時間まで取得してしまうため、
            // ・子プロジェクトの作業時間を削除する
            // ・重複して取得してしまったものを削除する
            return results.Where(r => projectIds.Contains(r.Project.Id))
                .Distinct()
                .ToList();
        }

        public DateTime GetStart(DateTime createAt)
        {
            switch (PeriodMode)
            {
                case PeriodType.LastWeek:
                    return createAt.AddDays(-7).Date;
                case PeriodType.LastMonth:
                    return createAt.AddDays(-30).Date;
                case PeriodType.SpecifyPeriod:
                    return Start.Date;
                default:
                    throw new NotSupportedException($"PeriodMode が {PeriodMode} はサポート対象外です。");
            }
        }

        public DateTime GetEnd(DateTime createAt)
        {
            switch (PeriodMode)
            {
                case PeriodType.LastWeek:
                case PeriodType.LastMonth:
                    return createAt.Date;
                case PeriodType.SpecifyPeriod:
                    return End.Date;
                default:
                    throw new NotSupportedException($"PeriodMode が {PeriodMode} はサポート対象外です。");
            }
        }

        public async Task<List<TicketModel>> GetTicketsAsync(List<TimeEntry> times, Managers.RedmineManager redmine)
        {
            if (times.Count == 0)
                return new List<TicketModel>();

            var issues = redmine.GetIssues(times.Select(t => t.Issue.Id.ToString()).Distinct().ToList());

            var allIssues = new List<Issue>();
            if (SpecifyParentIssue)
            {
                var children = redmine.GetChildIssues(ParentIssueId);
                children.Insert(0, redmine.GetIssue(ParentIssueId));
                foreach (var i in issues)
                {
                    if (allIssues.Any(a => a.Id == i.Id))
                        continue;

                    allIssues.Add(i);
                    if (i.ParentIssue == null || allIssues.Any(a => a.Id == i.ParentIssue.Id))
                        continue;

                    var child = i;
                    while (child.ParentIssue != null)
                    {
                        child = children.FirstOrDefault(a => a.Id == child.ParentIssue.Id);
                        if (child == null)
                            break;

                        if (allIssues.Any(a => a.Id == child.Id))
                            break;
                        else
                            allIssues.Add(child);
                    }
                }
            }
            else
            {
                var tmp = new List<Issue>();
                foreach (var i in issues)
                {
                    if (allIssues.Any(a => a.Id == i.Id))
                        continue;

                    allIssues.Add(i);
                    if (i.ParentIssue == null || allIssues.Any(a => a.Id == i.ParentIssue.Id))
                        continue;

                    var topIssueId = await redmine.GetTopIssueIdAsync(i.Id.ToString());
                    if (!topIssueId.HasValue)
                        continue;

                    if (tmp.Any(a => a.Id == topIssueId))
                    {
                        // すでに取得済みなら新たに取得は行わない
                        var child = i;
                        while (child.ParentIssue != null)
                        {
                            child = tmp.First(a => a.Id == child.ParentIssue.Id);
                            if (allIssues.Any(a => a.Id == child.Id))
                                break;
                            else
                                allIssues.Add(child);
                        }
                    }
                    else
                    {
                        // トップの親チケットを含む子チケットの一覧を取得
                        var children = redmine.GetChildIssues(topIssueId.ToString());
                        children.Insert(0, redmine.GetIssue(topIssueId.ToString()));

                        // 取得したチケットツリーは保持しておく
                        tmp.AddRange(children);

                        var child = i;
                        while (child.ParentIssue != null)
                        {
                            child = children.First(a => a.Id == child.ParentIssue.Id);
                            allIssues.Add(child);
                        }
                    }
                }
            }

            return allIssues.Select(a => new TicketModel(a)).ToList();
        }
    }
}
