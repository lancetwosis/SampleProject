using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Enums;
using Telerik.Windows.Controls;
using System.Windows.Data;
using LibRedminePower.Extentions;
using System.Collections.Concurrent;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System.Collections.ObjectModel;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Models.Bases;
using System.Threading;

namespace RedmineTableEditor.Models
{
    public class MyIssue : Bases.MyIssueBase
    {
        public Dictionary<int, List<MySubIssue>> ChildrenListDic { get; set; }
        public Dictionary<int, MySubIssue> ChildrenDic { get; set; }

        private RedmineManager redmine;
        private IEnumerable<SubIssueSettingModel> subIssueSettings;
        private Issue issue;

        public MyIssue(RedmineManager reminde, IEnumerable<SubIssueSettingModel> subIssues, Issue issue)
            : base(false, issue, reminde)
        {
            this.redmine = reminde;
            this.subIssueSettings = subIssues;
            this.issue = issue;

            ChildrenDic = new Dictionary<int, MySubIssue>();
            ChildrenListDic = new Dictionary<int, List<MySubIssue>>();
            foreach (var sub in subIssues)
            {
                var child = new MySubIssue(reminde, sub, null);
                ChildrenListDic.Add(sub.Order, new List<MySubIssue>() { child });
            }
        }

        public async Task UpdateChildrenAsync(CancellationToken token, bool isDetail)
        {
            if (subIssueSettings.Any())
            {
                // 子チケットを再帰的にすべて取得する。
                var rawIssues = await Task.Run(() => redmine.GetChildIssues(issue.Id)?.ToArray()).WithCancel(token);

                if (rawIssues != null && isDetail)
                {
                    var getDetailTasks = rawIssues.Select(a => Task.Run(() => redmine.GetIssue(a.Id)));
                    rawIssues = await Task.WhenAll(getDetailTasks).WithCancel(token);
                }

                foreach (var sub in subIssueSettings)
                {
                    ChildrenListDic[sub.Order] = rawIssues?.Where(a => sub.IsMatch(a))
                                                           .OrderBy(a => a.Id)
                                                           .Select(a => new MySubIssue(redmine, sub, a))
                                                           .ToList();
                    ChildrenDic[sub.Order] = null;
                }
            }
        }

        public override void Read()
        {
            // 自身の変更を読み込み
            base.Read();

            // 子チケットの変更を読み込み
            ChildrenDic.Values.Cast<MyIssueBase>().Where(a => a != null)
                .AsParallel().ForAll(a => a.Read());
        }

        public override void Write()
        {
            // 自身の変更を出力
            base.Write();

            // 子チケットの変更を出力
            ChildrenDic.Values.Cast<MyIssueBase>().Where(a => a != null && a.IsEdited.Value)
                .AsParallel().ForAll(a => a.Write());
        }

        /// <summary>
        /// 一つの親チケットに同名の子チケットが複数あった場合など、一つの親チケットに対して複数の行を設定する必要がある。
        /// </summary>
        public List<MyIssue> ToViewRows()
        {
            if (!subIssueSettings.Any() || !ChildrenListDic.Values.Where(a => a != null).Any())
                return new List<MyIssue>() { this };

            var max = ChildrenListDic.Values.Max(a => a.Count());
            if (max <= 0)
                return new List<MyIssue> { this };

            var result = new List<MyIssue>();
            foreach (var i in Enumerable.Range(0, max))
            {
                var parent = new MyIssue(redmine, subIssueSettings, Issue);
                foreach (var sub in subIssueSettings)
                {
                    if (ChildrenListDic[sub.Order].Count() >= i + 1)
                    {
                        parent.ChildrenDic.Add(sub.Order, ChildrenListDic[sub.Order][i]);
                    }
                    else
                    {
                        parent.ChildrenDic.Add(sub.Order, null);
                    }
                }
                result.Add(parent);
            }
            return result;
        }
    }
}
