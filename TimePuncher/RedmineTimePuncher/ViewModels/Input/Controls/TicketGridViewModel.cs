using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TelerikEx.PersistenceProvider;

namespace RedmineTimePuncher.ViewModels.Input.Controls
{
    public class TicketGridViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<string> Title { get; set; }
        public ReactiveProperty<List<MyIssue>> Items { get; set; }
        
        public ReactivePropertySlim<MyIssue> SelectedItem { get; set; }

        public BusyNotifier IsBusy { get; set; }
        public string ErrorMessage { get; set; }

        public MyQuery Query { get; set; }

        public AsyncCommandBase ReloadCommand { get; set; }
        public CommandBase GoToTicketCommand { get; set; }
        public CommandBase CopyRefsCommand { get; set; }
        public CommandBase AddFavoritesCommand { get; set; }
        public CommandBase RemoveFavoriteCommand { get; set; }

        public bool IsDragDropRunning { get; set; }

        public GridViewColumnProperties ColumnProperties { get; set; }

        private RedmineViewModel parent;

        public TicketGridViewModel(RedmineViewModel parent, string title, Func<List<MyIssue>> getItemsAction, GridViewColumnProperties gridViewProperties, bool isFavoriteGrid = false)
        {
            this.parent = parent;

            IsBusy = new BusyNotifier();
            Title = new ReactivePropertySlim<string>(title).AddTo(disposables);
            ColumnProperties = gridViewProperties != null ? gridViewProperties : new GridViewColumnProperties(title);

            Items = new ReactiveProperty<List<MyIssue>>(new List<MyIssue>()).AddTo(disposables);
            SelectedItem = new ReactivePropertySlim<MyIssue>().AddTo(disposables);

            var myDisposables = new CompositeDisposable();
            ReloadCommand = new AsyncCommandBase(
                Properties.Resources.IssueGridCmdUpdate, 'U', App.Current.Resources["GlyphReload"] as string,
                new ReactivePropertySlim<string>(),
                async () =>
                {
                    myDisposables?.Dispose();
                    myDisposables = new CompositeDisposable().AddTo(disposables);

                    using (IsBusy.ProcessStart())
                    {
                        Items.Value = null;
                        var t = Task.Run(() => getItemsAction.Invoke());
                        try
                        {
                            await t;
                            Items.Value = t.Result;
                            ErrorMessage = "";

                            if (isFavoriteGrid)
                            {
                                foreach (var i in Items.Value)
                                {
                                    i.IsFavorite = true;
                                    i.ObserveProperty(a => a.IsFavorite).Skip(1).SubscribeWithErr(isF =>
                                    {
                                        if (!isF)
                                            parent.RemoveFavoriteTicket(i);
                                    }).AddTo(myDisposables);
                                }

                                Properties.Settings.Default.FavoriteIssueIds = string.Join(",", Items.Value.Select(i => i.Id.ToString()));
                            }
                            else
                            {
                                foreach (var i in Items.Value)
                                {
                                    i.IsFavorite = Properties.Settings.Default.FavoriteIssueIds.Split(',').Contains(i.Id.ToString());
                                    i.ObserveProperty(a => a.IsFavorite).Skip(1).SubscribeWithErr(isF =>
                                    {
                                        if (isF)
                                            parent.AddFavoriteTicket(i);
                                        else
                                            parent.RemoveFavoriteTicket(i);
                                    }).AddTo(myDisposables);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = ex.Message;
                        }
                    }
                }).AddTo(disposables);

            GoToTicketCommand = new CommandBase(
                Properties.Resources.IssueGridCmdOpenIssue, 'O', Properties.Resources.icons8_linking_48,
                SelectedItem.Select(a => a != null ? null : ""),
                () => SelectedItem.Value.GoToTicket()).AddTo(disposables);

            CopyRefsCommand = new CommandBase(
                Properties.Resources.IssueGridCmdCopyAsRefs, 'C',
                SelectedItem.Select(a => a != null ? null : ""),
                () => Clipboard.SetData(DataFormats.Text, (Object)$"refs #{SelectedItem.Value.Id} {SelectedItem.Value.Subject}")).AddTo(disposables);

            AddFavoritesCommand = new CommandBase(
                Properties.Resources.IssueGridCmdAddFavorite, App.Current.Resources["GlyphStar"] as string,
                SelectedItem.Select(a => (a != null && !a.IsFavorite) ? null : ""),
                () => AddFavoriteTicket(SelectedItem.Value)).AddTo(disposables);

            RemoveFavoriteCommand = new CommandBase(
                Properties.Resources.IssueGridCmdRemoveFavorite, App.Current.Resources["GlyphStarOutline"] as string,
                SelectedItem.Select(a => (a != null && a.IsFavorite) ? null : ""),
                () => parent.RemoveFavoriteTicket(SelectedItem.Value)).AddTo(disposables);
        }

        public void AddFavoriteTicket(MyIssue issue)
        {
            parent.AddFavoriteTicket(issue);
        }
    }
}
