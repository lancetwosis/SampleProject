using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class QuerySettingsViewModel : Bases.SettingsViewModelBase<QuerySettingsModel>
    {
        public ReactivePropertySlim<bool> IsAutoUpdate { get; set; }
        public ReactivePropertySlim<int> AutoUpdateMinutes { get; set; }
        public TwinListBoxViewModel<MyQuery> TwinListBoxViewModel { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        private CompositeDisposable myDisposables;

        public QuerySettingsViewModel(QuerySettingsModel model, ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) :base(model)
        {
            var error1 = errorMessage.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var error2 = new ReactivePropertySlim<string>().AddTo(disposables);
            ErrorMessage = new IObservable<string>[] { error1, error2 }.CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var queries = new ReactivePropertySlim<List<MyQuery>>().AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                try
                {
                    error2.Value = Resources.SettingsMsgNowGettingData;
                    var q = await Task.Run(() => r.GetQueries());
                    queries.Value = q.Select(a => new MyQuery(a)).ToList();
                    error2.Value = "";
                }
                catch(Exception ex)
                {
                    error2.Value = ex.Message;
                }
            }).AddTo(disposables);

            queries.Where(a => a != null).Merge(ImportCommand).SubscribeWithErr(_ => setUpItemsSource(model, queries.Value)).AddTo(disposables);

            IsAutoUpdate = model.ToReactivePropertySlimAsSynchronized(m => m.IsAutoUpdate).AddTo(disposables);
            AutoUpdateMinutes = model.ToReactivePropertySlimAsSynchronized(m => m.AutoUpdateMinutes).AddTo(disposables);
        }

        private void setUpItemsSource(QuerySettingsModel model, List<MyQuery> queries)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            model.Items = new ObservableCollection<MyQuery>(model.Items.Select(a => queries.FirstOrDefault(b => a.Id == b.Id)).Where(a => a != null));
            var selectedItems = new ObservableCollection<MyQuery>(model.Items);
            TwinListBoxViewModel = new TwinListBoxViewModel<MyQuery>(queries, selectedItems);
            selectedItems.ObserveAddChanged().SubscribeWithErr(a => model.Items.Add(a)).AddTo(myDisposables);
            selectedItems.ObserveRemoveChanged().SubscribeWithErr(a => model.Items.Remove(a)).AddTo(myDisposables);
            selectedItems.CollectionChangedAsObservable().Where(a => a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move).SubscribeWithErr(a =>
            {
                model.Items.Move(a.OldStartingIndex, a.NewStartingIndex);
            }).AddTo(myDisposables);
        }
    }
}
