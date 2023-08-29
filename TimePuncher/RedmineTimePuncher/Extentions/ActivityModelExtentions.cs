using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Extentions
{
    public static class ActivityModelExtentions
    {
        public static async Task<(MyIssue, Journal)> ToIssueAtTheTimeAsync(this ActivityModel activity, RedmineManager redmine)
        {
            // 変更履歴つきのチケット情報を取得する。
            var issue = await Task.Run(() => redmine.GetIssueIncludeJournal(activity.IssueId));
            Journal journal = null;
            // ChangeNoが空の場合は、
            if (string.IsNullOrEmpty(activity.ChangeNo))
            {
                // 分単位でチケット作成時と同じであれば、チケット作成時の活動ログと判断する。
                if (activity.Date - issue.CreatedOn.Value <= TimeSpan.FromMinutes(1))
                {
                    activity.Date = issue.CreatedOn.Value;
                }
            }
            else
            {
                // 変更履歴番号があるのに、変更履歴がヒットしない場合は、
                if (issue.Journals == null || !issue.Journals.Any(a => a.Id.ToString() == activity.ChangeNo))
                {
                    // キャッシュが古い可能性があるので、改めて取得しなおす
                    issue = await Task.Run(() => redmine.GetIssueIncludeJournal(activity.IssueId, true));
                }

                // 変更履歴から活動の日時を更新する。
                journal = issue.Journals.First(a => a.Id.ToString() == activity.ChangeNo);
                activity.Date = journal.CreatedOn.Value;
            }

            // 活動日時から当時のチケットを復元する。
            var result = redmine.RestoreJournals(issue, activity.Date);
            return (result, journal);
        }
    }
}
