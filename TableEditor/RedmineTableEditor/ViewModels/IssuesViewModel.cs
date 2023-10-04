using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
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

        public AutoBackColorModel AutoBackColor { get; set; }
        public ReactivePropertySlim<Models.RedmineManager> Redmine => parent.Redmine;

        public MyIssueBase CurrentIssue { get; set; }
        public ObservableCollection<MyIssueBase> SelectedIsuues { get; set; }

        public ReactiveCommand ReadTicketCommand { get; set; }
        public ReactiveCommand GotoTicketCommand { get; set; }

        private TableEditorViewModel parent;
        private RadObservableCollection<MyIssue> parentIssues;
        private RadObservableCollection<MyIssuePivot> allChildren;
        private FileSettingsModel lastFileSettings;

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
            parentIssues.ObserveResetChanged().Subscribe(_ => editedIssues.Clear());
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

            //----------------------------
            // 表示条件の適用
            //----------------------------
            parent.ApplyCommand = new AsyncCommandBase(
                Resources.RibbonCmdApply, Resources.apply_icon,
                new[] {
                    Redmine.Select(a => a != null ? a.IsValid() : ""),
                    parent.IsBusy.Select(a => !a ? null : ""),
                    parent.ObserveProperty(p => p.FileSettings.ParentIssues.Value.IsValid.Value),
                }.CombineLatest().Select(a => a.FirstOrDefault(m => m != null)),
                async () =>
                {
                    await ApplyAsync(parent.FileSettings.Model.Value);
                }).AddTo(disposables);

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
            // チケット読み込み
            //----------------------------
            ReadTicketCommand = this.ObserveProperty(a => a.CurrentIssue).Select(a => a != null).ToReactiveCommand().WithSubscribe(async () =>
            {
                try
                {
                    using (parent.IsBusy.ProcessStart())
                    {
                        await Task.Run(() =>
                        SelectedIsuues.ToList().AsParallel().ForAll(a => a.Read()));
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

        public async Task ApplyAsync(FileSettingsModel fileSettings)
        {
            parent.CTS = new CancellationTokenSource();
            using (parent.IsBusy.ProcessStart())
            {
                // ファイル設定
                var readAllFlag = true;

                parent.CTS.Token.ThrowIfCancellationRequested();

                // 自動色設定
                if (lastFileSettings == null ||
                    lastFileSettings.AutoBackColor.ToJson() != fileSettings.AutoBackColor.ToJson())
                {
                    AutoBackColor = fileSettings.AutoBackColor.Clone();
                    readAllFlag = false;
                }

                parent.CTS.Token.ThrowIfCancellationRequested();

                // カラムを作成する。
                if (lastFileSettings == null ||
                    lastFileSettings.ParentIssues.Properties.ToJson() != fileSettings.ParentIssues.Properties.ToJson() ||
                    lastFileSettings.SubIssues.ToJson() != fileSettings.SubIssues.ToJson())
                {
                    // カラムグループを作成する。
                    ColumnGroups = fileSettings.SubIssues.Items.OrderBy(a => a.Order).Where(a => a.IsEnabled).Select(b => new GridViewColumnGroup()
                    {
                        Name = b.Order.ToString(),
                        Header = b.Title
                    }).ToList();

                    // カラム作成。LeftFrozenColumnCount を正常に作動させるために一旦クリアしてから追加する
                    Columns = new List<GridViewBoundColumnBase>();
                    var columns = fileSettings.ParentIssues.Properties.Select(p => p.CreateColumn(Redmine.Value)).Where(a => a != null).ToList();
                    LeftFrozenColumnCount = columns.Count();
                    foreach (var g in fileSettings.SubIssues.Items.Where(a => a.IsEnabled && a.IsValid).OrderBy(a => a.Order))
                    {
                        columns.AddRange(fileSettings.SubIssues.Properties
                            .Where(a => a.Field.HasValue || Redmine.Value.CustomFields.Any(b => b.Id == a.CustomFieldId))
                            .Select(a => a.CreateColumn(Redmine.Value, g.Order))
                            .Where(a => a != null));
                    }
                    Columns = columns;
                    readAllFlag = false;
                }

                parent.CTS.Token.ThrowIfCancellationRequested();

                // 一覧に影響があるものが更新された場合のみ全更新する。
                if (readAllFlag ||
                    lastFileSettings == null ||
                    lastFileSettings.ParentIssues.ToJson() != fileSettings.ParentIssues.ToJson() ||
                    lastFileSettings.SubIssues.Items.ToJson() != fileSettings.SubIssues.Items.ToJson())
                {
                    clearIssues();

                    // 親チケットを取得する。
                    var myItems = await fileSettings.ParentIssues.GetIssuesAsync(Redmine.Value, parent.CTS.Token);
                    if (myItems != null && myItems.Any())
                    {
                        var issues = myItems.Select(issue => new MyIssue(Redmine.Value, fileSettings.SubIssues.Items, issue)).ToList();
                        if (fileSettings.ParentIssues.UseQuery)
                            parentIssues.AddRange(issues);
                        else
                            if (fileSettings.ParentIssues.Recoursive)
                                parentIssues.AddRange(orderByParentChild(fileSettings.ParentIssues, issues));
                            else
                                parentIssues.AddRange(issues.OrderBy(i => i.Id));

                        // 子チケットの情報を取得する。
                        var needsDetail = fileSettings.SubIssues.Properties.Any(a => a.IsDetail());
                        await Task.WhenAll(parentIssues.Select(a => a.UpdateChildrenAsync(parent.CTS.Token, needsDetail)));

                        IssuesView.AddRange(parentIssues.SelectMany(a => a.ToViewRows()));
                        IssuesView.SelectMany(a => a.ChildrenDic.Values).Where(a => a != null)
                            .Select(a => new MyIssuePivot(a)).ToList().ForEach(a => allChildren.Add(a));

                        LocalData = new LocalDataSourceProvider();
                        LocalData.ItemsSource = allChildren;

                        allChildren.Select(a => a.PropertyChangedAsObservable()).Merge().ObserveOnUIDispatcher().Subscribe(_ => LocalData.Refresh());
                    }
                }

                lastFileSettings = parent.FileSettings.Model.Value.Clone();
            }
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

        private void clearIssues()
        {
            parentIssues.Clear();
            IssuesView.Clear();
            allChildren.Clear();
            MyIssueBase.EstimatedHoursMax = 0;
            MyIssueBase.SpentHoursMax = 0;
            MyIssueBase.MySpentHoursMax = 0;
            MyIssueBase.DiffEstimatedSpentMax = 0;
        }

        public void Clear()
        {
            ColumnGroups = new List<GridViewColumnGroup>();
            Columns = new List<GridViewBoundColumnBase>();

            clearIssues();

            lastFileSettings = null;
        }
    }
}
