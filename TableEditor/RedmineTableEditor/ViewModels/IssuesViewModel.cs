using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.Bases;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Properties;
using RedmineTableEditor.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Pivot.Core;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace RedmineTableEditor.ViewModels
{
    public class IssuesViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public RadObservableCollection<MyIssue> IssuesView { get; set; }
        public LocalDataSourceProvider LocalData { get; set; }

        public int LeftFrozenColumnCount { get; set; }

        public List<GridViewColumnGroup> ColumnGroups { get; set; }
        public List<GridViewBoundColumnBase> Columns { get; set; }
        public ReactivePropertySlim<GridViewColumn> CurrentColumn { get; set; }

        public AutoBackColorModel AutoBackColor { get; set; }
        public ReactivePropertySlim<Models.RedmineManager> Redmine => parent.Redmine;

        public MyIssueBase CurrentIssue { get; set; }
        public ObservableCollection<MyIssueBase> SelectedIsuues { get; set; }

        public ReactiveCommand ReadTicketCommand { get; set; }
        public ReactiveCommand GotoTicketCommand { get; set; }

        private TableEditorViewModel parent;
        private RadObservableCollection<MyIssue> parentIssues;
        private RadObservableCollection<MyIssuePivot> allChildren;

        public IssuesViewModel(TableEditorViewModel parent)
        {
            this.parent = parent;
            parentIssues = new RadObservableCollection<MyIssue>();
            allChildren = new RadObservableCollection<MyIssuePivot>();

            IssuesView = new RadObservableCollection<MyIssue>();
            SelectedIsuues = new ObservableCollection<MyIssueBase>();

            LocalData = new LocalDataSourceProvider();
            LocalData.ItemsSource = allChildren;

            var editedIssues = new ObservableCollection<MyIssueBase>();
            parentIssues.ObserveResetChanged().SubscribeWithErr(_ => editedIssues.Clear());
            MyIssueEditedListener.EditedChanged = (i, b) =>
            {
                if (b)
                {
                    if (!editedIssues.Contains(i))
                        editedIssues.Add(i);
                }
                else
                {
                    if (editedIssues.Contains(i))
                        editedIssues.Remove(i);
                }
            };

            CurrentColumn = new ReactivePropertySlim<GridViewColumn>().AddTo(disposables);

            //----------------------------
            // 表示条件の適用
            //----------------------------
            var canApply = new[] {
                    Redmine.Select(a => a != null ? a.IsValid() : ""),
                    parent.IsBusy.Select(a => !a ? null : ""),
                    parent.ObserveProperty(p => p.FileSettings.ParentIssues.Value.IsValid.Value),
                }.CombineLatest().Select(a => a.FirstOrDefault(m => m != null));
            parent.ApplyCommand = new AsyncCommandBase(
                Resources.RibbonCmdApply, Resources.apply_icon,
                canApply,
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(parent.ApplyCommand));
                    await ApplyAsync(parent.FileSettings.Model.Value, false);
                },
                new ReactivePropertySlim<List<ChildCommand>>(new List<ChildCommand>()
                {
                    new ChildCommand(Resources.RibbonCmdApplyAll, canApply,
                    async () =>
                    {
                        TraceHelper.TrackCommand("ApplyAndUpdateCacheCommand");
                        await ApplyAsync(parent.FileSettings.Model.Value, true);
                    }),
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables)).AddTo(disposables);

            //----------------------------
            // チケットの内容の読み込み
            //----------------------------
            parent.UpdateContentCommand = new AsyncCommandBase(
                Resources.RibbonCmdUpdate, Resources.reload_icon,
                new[] {
                    Redmine.Select(a => a != null && string.IsNullOrEmpty(a.IsValid())),
                    parent.IsBusy.Select(a => !a),
                    parentIssues.AnyAsObservable(),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(parent.UpdateContentCommand));
                    using (parent.IsBusy.ProcessStart())
                    {
                        var allIssues = IssuesView.SelectMany(i => i.ChildrenDic.Values).Where(a => a != null).Cast<MyIssueBase>()
                            .Concat(parentIssues.ToList()).Where(a => a.Issue != null).ToList();
                        var rawIssues = await Task.Run(() => Redmine.Value.GetIssues(allIssues.Select(a => a.Id.Value)));
                        allIssues.ToList().AsParallel().ForAll(a => a.SetIssue(rawIssues.SingleOrDefault(b => b.Id == a.Id)));
                    }
                }).AddTo(disposables);

            //----------------------------
            // チケットの編集内容の Redmine への反映
            //----------------------------
            parent.SaveToRedmineCommand = new AsyncCommandBase(
                Resources.RibbonCmdSaveRedmine, Resources.save_redmine_icon,
                new[] {
                    Redmine.Select(a => a != null && string.IsNullOrEmpty(a.IsValid())),
                    parent.IsBusy.Select(a => !a),
                    editedIssues.AnyAsObservable(),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(parent.SaveToRedmineCommand));
                    try
                    {
                        using (parent.IsBusy.ProcessStart())
                        {
                            await Task.Run(() => editedIssues.ToList().AsParallel().ForAll(a => a.Write()));
                        }
                    }
                    catch (AggregateException ex)
                    {
                        var errorList = string.Join("", ex.InnerExceptions.Select(a => a.Message));
                        throw new ApplicationException(string.Format(Resources.ErrMsgFailedUpdateIssues, errorList));
                    }
                }).AddTo(disposables);

            //----------------------------
            // チケット一覧の Redmine での表示
            //----------------------------
            parent.ShowOnRedmineCommand = new CommandBase(
                Resources.RibbonCmdShowRedmine, Resources.icons8_show_on_redmine_48,
                canApply,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(parent.ShowOnRedmineCommand));
                    parent.FileSettings.ParentIssues.Value.ShowIssuesOnRedmine();
                }).AddTo(disposables);

            //----------------------------
            // 左端のカラムの固定
            //----------------------------
            parent.SetFrozenColumnCommand = new CommandBase(
                Resources.RibbonCmdSetFrozenColumn, Resources.icons8_select_column_32,
                CurrentColumn.Select(a => a != null ? null : ""),
                () =>
                {
                    var frozenIndex = CurrentColumn.Value.DisplayIndex + 1;
                    if (parent.FileSettings.Model.Value.FrozenColumnCount == frozenIndex)
                        return;

                    parent.FileSettings.Model.Value.FrozenColumnCount = frozenIndex;
                    parent.FileSettings.IsEdited.Value = true;

                    var issues = parentIssues.ToList();
                    Clear();
                    updateColumns(parent.FileSettings.Model.Value);
                    parentIssues.AddRange(issues);
                    updateView();
                }).AddTo(disposables);

            //----------------------------
            // チケット読み込み
            //----------------------------
            ReadTicketCommand = this.ObserveProperty(a => a.CurrentIssue).Select(a => a != null).ToReactiveCommand().WithSubscribe(async () =>
            {
                try
                {
                    using (parent.IsBusy.ProcessStart())
                    {
                        await Task.Run(() => SelectedIsuues.ToList().AsParallel().ForAll(a => a.Read()));
                    }
                }
                catch (AggregateException ex)
                {
                    var errorList = string.Join("", ex.InnerExceptions.Select(a => a.Message));
                    throw new ApplicationException(string.Format(Resources.ErrMsgFailedReadIssues, errorList));
                }
            }).AddTo(disposables);

            //----------------------------
            // チケットへジャンプ
            //----------------------------
            GotoTicketCommand = this.ObserveProperty(a => a.CurrentIssue).Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                if (SelectedIsuues.Count() > 1)
                {
                    var ids = string.Join(",", SelectedIsuues.Select(a => a.Id));
                    var url = new Uri(new Uri(Redmine.Value.UrlBase), $"projects/{CurrentIssue.Issue.Project.Id}/issues?status_id=*&set_filter=1&issue_id={ids}");
                    System.Diagnostics.Process.Start(url.AbsoluteUri);
                }
                else
                {
                    System.Diagnostics.Process.Start(CurrentIssue.Url);
                }
            }).AddTo(disposables);
        }

        private void updateColumns(FileSettingsModel fileSettings)
        {
            // カラムグループを作成する。
            ColumnGroups = fileSettings.SubIssues.CreateSubIssueColumnGroups();

            var columns = fileSettings.ParentIssues.CreateColumns(Redmine.Value)
                .Concat(fileSettings.SubIssues.CreateColumns(Redmine.Value)).ToList();

            if (columns.Count < fileSettings.FrozenColumnCount)
            {
                fileSettings.FrozenColumnCount = FileSettingsModel.DEFAULT_FROZEN_COUNT;
                parent.FileSettings.IsEdited.Value = true;
            }

            // カラム作成。LeftFrozenColumnCount を正常に作動させるために一旦クリアしてから追加する
            Columns = new List<GridViewBoundColumnBase>();
            LeftFrozenColumnCount = fileSettings.FrozenColumnCount;
            Columns = columns;
        }

        public async Task ApplyAsync(FileSettingsModel fileSettings, bool updateCache)
        {
            parent.CTS = new CancellationTokenSource();
            using (parent.IsBusy.ProcessStart())
            {
                Clear();

                // 親チケットを取得する。
                var rawIssues = await fileSettings.ParentIssues.GetIssuesAsync(Redmine.Value, parent.CTS.Token);
                if (rawIssues != null && rawIssues.Any())
                    await Task.Run(() => Redmine.Value.UpdateAsync(rawIssues, updateCache));
                else
                    return;

                parent.CTS.Token.ThrowIfCancellationRequested();

                // 自動色設定
                AutoBackColor = fileSettings.AutoBackColor.Clone();

                // カラムを更新
                updateColumns(fileSettings);

                parent.CTS.Token.ThrowIfCancellationRequested();

                // 親チケットを設定する。
                var issues = rawIssues.Select(issue => new MyIssue(Redmine.Value, fileSettings, issue)).ToList();
                if (fileSettings.ParentIssues.UseQuery)
                    parentIssues.AddRange(issues);
                else
                    parentIssues.AddRange(issues.OrderBy(i => i.Id));
                    // TODO: 親子関係でのソートを対応するときに整理すること！
                    //if (fileSettings.ParentIssues.Recoursive)
                    //    parentIssues.AddRange(orderByParentChild(fileSettings.ParentIssues, issues));
                    //else
                    //    parentIssues.AddRange(issues.OrderBy(i => i.Id));

                // 子チケットの情報を取得する。
                await Task.WhenAll(parentIssues.Select(a => a.UpdateChildrenAsync(parent.CTS.Token)));

                updateView();
            }
        }

        private void updateView()
        {
            IssuesView.AddRange(parentIssues.SelectMany(a => a.ToViewRows()));
            IssuesView.SelectMany(a => a.ChildrenDic.Values).Where(a => a != null)
                .Select(a => new MyIssuePivot(a)).ToList().ForEach(a => allChildren.Add(a));

            LocalData = new LocalDataSourceProvider();
            LocalData.ItemsSource = allChildren;

            allChildren.Select(a => a.PropertyChangedAsObservable()).Merge().ObserveOnUIDispatcher().SubscribeWithErr(_ => LocalData.Refresh());
        }

        private List<MyIssue> orderByParentChild(ParentIssueSettingsModel setting, List<MyIssue> issues)
        {
            // 親チケットの下に子チケットが来るようにソートする
            var parentId = int.Parse(setting.IssueId);
            var tree = new Dictionary<int, List<MyIssue>>();
            foreach (var i in issues.OrderBy(i => i.Id))
            {
                if (i.Id == parentId)
                    tree[0] = new List<MyIssue>() { i };
                else if (tree.ContainsKey(i.Issue.ParentIssue.Id))
                    tree[i.Issue.ParentIssue.Id].Add(i);
                else
                    tree[i.Issue.ParentIssue.Id] = new List<MyIssue>() { i };
            }

            var results = new List<MyIssue>();
            var tops = tree[setting.ShowParentIssue ? 0 : parentId];
            foreach (var t in tops)
            {
                orderSub(tree, results, t, 0);
            }
            return results;
        }

        private void orderSub(Dictionary<int, List<MyIssue>> tree, List<MyIssue> results, MyIssue i, int depth)
        {
            i.Depth = depth;
            results.Add(i);
            if (tree.ContainsKey(i.Id.Value))
            {
                foreach (var child in tree[i.Id.Value])
                {
                    orderSub(tree, results, child, depth + 1);
                }
            }
        }

        public void Clear()
        {
            ColumnGroups = new List<GridViewColumnGroup>();
            Columns = new List<GridViewBoundColumnBase>();

            parentIssues.Clear();
            IssuesView.Clear();
            allChildren.Clear();
            MyIssueBase.EstimatedHoursMax = 0;
            MyIssueBase.SpentHoursMax = 0;
            MyIssueBase.MySpentHoursMax = 0;
            MyIssueBase.DiffEstimatedSpentMax = 0;
        }
    }
}
