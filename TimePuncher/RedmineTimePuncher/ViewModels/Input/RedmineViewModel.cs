using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Input.Controls;
using RedmineTimePuncher.ViewModels.Input.Bases;
using RedmineTimePuncher.ViewModels.Input.Resources;
using RedmineTimePuncher.ViewModels.Input.Slots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using LibRedminePower;
using RedmineTimePuncher.Models.Managers;
using Redmine.Net.Api.Types;
using TelerikEx.PersistenceProvider;
using Newtonsoft.Json;
using RedmineTimePuncher.Models.Settings;
using System.Reactive.Disposables;

namespace RedmineTimePuncher.ViewModels.Input
{
    public class RedmineViewModel : ResourceViewModelBase<RedmineResource>
    {
        public BusyTextNotifier IsBusyTicketList { get; set; }

        public ReadOnlyReactivePropertySlim<ReactiveTimer> Timer { get; set; }
        public ReadOnlyReactivePropertySlim<ReactiveTimer> QueryTimer { get; set; }

        public CommandBase GoToTicketCommand { get; }

        public ReadOnlyReactiveCollection<ResourceSettingViewModel> ResourceSettingList { get; set; }
        public ReadOnlyReactivePropertySlim<Func<object, bool>> GroupFilter { get; set; }
        public ReactivePropertySlim<List<TicketGridViewModel>> TicketList { get; set; }
        public int SelectedTicketGridIndex { get; set; }
        public TicketGridViewModel FavoriteTickets { get; set; }
        public CategoryListBoxViewModel CategoryListBoxViewModel { get; set; }

        public InputViewModel Parent { get; set; }

        public RedmineViewModel(InputViewModel parent) : base()
        {
            Parent = parent;

            Resource = new RedmineResource(parent.UrlBase, parent.Parent.Redmine);

            IsBusyTicketList = new BusyTextNotifier();

            var columnPropsList = JsonConvert.DeserializeObject<List<GridViewColumnProperties>>(Properties.Settings.Default.TicketGridViews);
            if( columnPropsList == null ) columnPropsList = new List<GridViewColumnProperties>();

            // チケット一覧を更新する
            TicketList = new ReactivePropertySlim<List<TicketGridViewModel>>().AddTo(disposables);
            parent.Parent.Redmine.Where(a => a != null).CombineLatest(
                parent.Parent.Settings.ObserveProperty(a => a.Query), CacheManager.Default.Updated, (r, q, _) =>
            {
                using (IsBusyTicketList.ProcessStart(Properties.Resources.ProgressMsgGettingIssues))
                {
                    // クエリ追加
                    var queries = CacheManager.Default.Queries;
                    var grids = q.Items.Where(a => queries.Any(b => a.Id == b.Id)).Select(query =>
                    {
                        return new TicketGridViewModel(this, query.Name,
                            () => r.GetTicketsByQuery(query).ToList(), 
                            columnPropsList.SingleOrDefault(g => g.UniqueName == query.Name));
                    }).ToList();

                    // マイチケットを先頭に追加
                    grids.Insert(0, new TicketGridViewModel(this, Properties.Resources.IssueGridMyIssues, 
                        () => r.GetMyTickets().OrderBy(b => b).ToList(),
                        columnPropsList.SingleOrDefault(g => g.UniqueName == Properties.Resources.IssueGridMyIssues)));

                    // お気に入りを2番目に追加
                    FavoriteTickets = new TicketGridViewModel(this, Properties.Resources.IssueGridFavorites, 
                        () =>
                        {
                            if (string.IsNullOrEmpty(Properties.Settings.Default.FavoriteIssueIds))
                                return new List<MyIssue>();

                            return r.GetTicketsByIds(Properties.Settings.Default.FavoriteIssueIds.Split(',').ToList()).ToList();
                        },
                        columnPropsList.SingleOrDefault(g => g.UniqueName == Properties.Resources.IssueGridFavorites), true);
                    grids.Insert(1, FavoriteTickets);

                    // 非同期でチケットを取得する
                    var __ = grids.Select(g => g.ReloadCommand.Command.ExecuteAsync()).ToArray();

                    return grids;
                }
            }).SubscribeWithErr(a =>
            {
                TicketList.Value = a;

                var index = Properties.Settings.Default.SelectedTicketGridIndex;
                SelectedTicketGridIndex = index < TicketList.Value.Count ? index : 0;
            }).AddTo(disposables);

            // カテゴリ情報を作成する。
            CategoryListBoxViewModel = new CategoryListBoxViewModel(parent).AddTo(disposables);
            parent.Parent.Redmine.CombineLatest(parent.Parent.Settings.ObserveProperty(a => a.Category), CacheManager.Default.Updated,
                (r, c, _) => (r, c)).SubscribeWithErr(p =>
            {
                if (p.r != null)
                {
                    p.c.UpdateItems(CacheManager.Default.TimeEntryActivities);
                    parent.Parent.Settings.Save();
                }

                var settings = p.c.Items.Where(a => a.IsEnabled).ToList();
                CategoryListBoxViewModel.AllSettings.Value = settings;

                var categories = new List<MyCategory>();
                if (p.r != null)
                {
                    // すべてのプロジェクトの時間管理に設定されている「作業分類」を対象とする。
                    // これにより「システム作業分類」のチェックが外れているものも対象にできる。
                    var entryActivities = CacheManager.Default.Projects.Where(pro => pro.TimeEntryActivities != null)
                                                                       .SelectMany(pro => pro.TimeEntryActivities)
                                                                       .Distinct((e1, e2) => e1.Id == e2.Id).ToList();
                    foreach (var entryActivity in entryActivities)
                    {
                        var categorySetting = settings.FirstOrDefault(a => a.Name == entryActivity.Name);
                        if (categorySetting != null)
                            categories.Add(new MyCategory(categorySetting, entryActivity));
                    }
                }
                else
                {
                    categories = settings.Select(c => new MyCategory(c)).ToList();
                }
                MyAppointment.AllCategories.Value = categories;
            }).AddTo(disposables);

            // Updaterを作成
            Timer = parent.Parent.Settings.ObserveProperty(a => a.Appointment.Redmine)
                .Select(a => Resource.Updater.CreateAutoReloadTimer(a))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            QueryTimer = parent.Parent.Settings.ObserveProperty(a => a.Query)
                .Select(a => Resource.Updater.CreateAutoReloadTimer(a, () =>
                    TicketList.Value != null ? Task.WhenAll(TicketList.Value.Select(t => t.ReloadCommand.Command.ExecuteAsync())) : Task.CompletedTask))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Resource.Updater.SetUpdateCommand(parent.Parent.Redmine.Select(a => a != null), async (ct) =>
            {
                await execUpdateAsync(parent, async () =>
                {
                    Logger.Info("redmineResource.SetReloadCommand Start");
                    var result = await parent.Parent.Redmine.Value.GetActivityAposAsync(ct, parent.Redmine.Resource, parent.StartTime.Value, parent.EndTime.Value);
                    parent.Appointments.RemoveAll(a => a.Resources.Contains(parent.Redmine.Resource));
                    parent.Appointments.AddRange(parent.Parent.Settings.Appointment.Redmine.Filter(result));
                    Logger.Info("redmineResource.SetReloadCommand End");
                });
            });

            // Redmineチケットを開く
            GoToTicketCommand = new CommandBase(
                Properties.Resources.RibbonCmdOpenIssue, 'O', Properties.Resources.icons8_linking_48,
                new[] {
                    parent.SelectedAppointments.Select(a => a == null || !a.Any()).Select(a => a ? Properties.Resources.msgErrSelectAppointments : null),
                    parent.SelectedAppointments.Select(a => a != null && !a.Any(b => b.GotoTicketCommand.CanExecute())).Select(a  => a ? Properties.Resources.msgErrSetTicket : null),
                }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(GoToTicketCommand) + ".Executed");

                    if (parent.SelectedAppointments.Value.Count() > 1)
                    {
                        var ids = string.Join(",", parent.SelectedAppointments.Value.Where(a => !string.IsNullOrEmpty(a.TicketNo)).Select(a => a.TicketNo));
                        var url = new Uri(new Uri(parent.Parent.Redmine.Value.Host), $"issues?status_id=*&set_filter=1&issue_id={ids}");
                        System.Diagnostics.Process.Start(url.AbsoluteUri);
                    }
                    else
                    {
                        parent.SelectedAppointments.Value.Where(a => a.GotoTicketCommand.CanExecute()).ToList().ForEach(apo =>
                        apo.GotoTicketCommand.Execute());
                    }
                }).AddTo(disposables);
        }

        private MyIssue getFavoriteIssue(RedmineManager r, string id)
        {
            try
            {
                var favorite = r.GetTicketsById(id);
                favorite.IsFavorite = true;
                return favorite;
            }
            catch
            {
                Logger.Warn($"Failed to getFavoriteIssue (Id={id})");
                return null;
            }
        }

        public void AddFavoriteTicket(MyIssue issue)
        {
            if (TicketList.Value == null) return;

            var favorites = FavoriteTickets.Items.Value != null ? FavoriteTickets.Items.Value.ToList() : new List<MyIssue>();
            if (!favorites.Any(t => t.Id == issue.Id))
            {
                var favorite = getFavoriteIssue(Parent.Parent.Redmine.Value, issue.Id.ToString());
                if (favorite != null)
                {
                    favorite.ObserveProperty(a => a.IsFavorite).Skip(1).SubscribeWithErr(isF =>
                    {
                        if (!isF)
                        {
                            RemoveFavoriteTicket(favorite);
                        }
                    });

                    favorites.Add(favorite);
                }
            }
            FavoriteTickets.Items.Value = favorites;

            foreach (var i in TicketList.Value.Where(g => g.Items.Value != null).SelectMany(g => g.Items.Value).Where(i => i.Id == issue.Id))
            {
                i.IsFavorite = true;
            }

            Properties.Settings.Default.FavoriteIssueIds = string.Join(",", favorites.Select(t => t.Id.ToString()));
        }

        public void RemoveFavoriteTicket(MyIssue issue)
        {
            if (TicketList.Value == null) return;

            var favorites =  new List<MyIssue>();
            if (FavoriteTickets.Items.Value != null)
            {
                favorites = FavoriteTickets.Items.Value.Where(t => t.Id != issue.Id).ToList();
                FavoriteTickets.Items.Value.FirstOrDefault(t => t.Id == issue.Id)?.Dispose();
            }
            FavoriteTickets.Items.Value = favorites;

            foreach (var i in TicketList.Value.Where(g => g.Items.Value != null).SelectMany(g => g.Items.Value).Where(i => i.Id == issue.Id))
            {
                i.IsFavorite = false;
            }

            Properties.Settings.Default.FavoriteIssueIds = string.Join(",", favorites.Select(t => t.Id.ToString()));
        }
    }
}
