using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using RedmineTimePuncher.ViewModels.Input.Bases;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.TreeMap;

namespace RedmineTimePuncher.ViewModels.ShowTimeEntry
{
    public class ShowTimeEntryViewModel : FunctionViewModelBase
    {
        public ReadOnlyReactivePropertySlim<GroupDefinitionCollection> GroupDefinitions { get; set; }
        public ObservableCollection<GroupingType> SelectedGroupDefinitions { get; }
        public ReactivePropertySlim<DateTime> StartDate { get; set; }
        public ReactivePropertySlim<DateTime> EndDate { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyUser>> Users { get; set; }
        public ObservableCollection<MyUser> SelectedUsers { get; set; }
        public ReadOnlyReactivePropertySlim<string> SelectedUsersTooltip { get; set; }

        public AsyncCommandBase ReloadCommand { get; set; }

        public ReactivePropertySlim<List<MyTimeEntry>> TimeEntries { get; set; } = new ReactivePropertySlim<List<MyTimeEntry>>();

        public ShowTimeEntryViewModel(MainWindowViewModel parent) : base(ApplicationMode.EntryViewer, parent)
        {
            SelectedGroupDefinitions = new ObservableCollection<GroupingType>();
            GroupDefinitions = SelectedGroupDefinitions.CollectionChangedAsObservable().Select(_ =>
            {
                var collection = new GroupDefinitionCollection();
                SelectedGroupDefinitions.ToList().ForEach(a => collection.Add(a.ToGroupDefinition()));
                return collection;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            StartDate = new ReactivePropertySlim<DateTime>(DateTime.Today.AddDays(-30)).AddTo(disposables);
            EndDate = new ReactivePropertySlim<DateTime>(DateTime.Today).AddTo(disposables);

            Users = parent.Redmine.CombineLatest(parent.Settings.ObserveProperty(a => a.User), (r, u) =>
            {
                var users = new List<MyUser>();
                if (r != null)
                {
                    users.Add(parent.Redmine.Value.MyUser);
                    u.Items.ToList().ForEach(i => users.Add(i));
                }
                return users;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            SelectedUsers = new ObservableCollection<MyUser>();
            SelectedUsersTooltip = SelectedUsers.CollectionChangedAsObservable().Select(_ => string.Join(", ", SelectedUsers)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            ReloadCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdLoad, Properties.Resources.reload,
                StartDate.CombineLatest(EndDate, (s, e) => s != DateTime.MinValue && e != DateTime.MinValue ? null : ""),
                async () =>
                {
                    // 指定された期間の予定を全て取得する。
                    var errorIds = new List<int>();
                    TimeEntries.Value = await Task.Run(() => parent.Redmine.Value.GetTimeEntries(SelectedUsers.Select(u => u.Id.ToString()).ToList(), StartDate.Value, EndDate.Value, out errorIds));
                });
        }
    }
}
