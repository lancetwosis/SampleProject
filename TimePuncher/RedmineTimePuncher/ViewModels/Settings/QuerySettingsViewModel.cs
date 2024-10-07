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
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReactivePropertySlim<bool> IsAutoUpdate { get; set; }
        public ReactivePropertySlim<int> AutoUpdateMinutes { get; set; }
        public ReadOnlyReactivePropertySlim<TwinListBoxViewModel<MyQuery>> TwinListBoxViewModel { get; set; }

        public QuerySettingsViewModel(QuerySettingsModel model) :base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsAutoUpdate = model.ToReactivePropertySlimAsSynchronized(m => m.IsAutoUpdate).AddTo(disposables);
            AutoUpdateMinutes = model.ToReactivePropertySlimAsSynchronized(m => m.AutoUpdateMinutes).AddTo(disposables);
            TwinListBoxViewModel =
                // ModelのItemsが更新されたら（インポートなどでも）
                model.ObserveProperty(a => a.Items).CombineLatest(
                    // かつ、Cacheが更新されたら
                    CacheTempManager.Default.MyQueries.Where(a => a != null),
                    (items, queries) => {
                        // まずは、RedmineのQueiresで削除された物は、ModelのItemsから削除する。
                        model.UpdateItems(queries);
                        // 次に、更新したModelのItemsから、TiwnListBoxのViewModelを作る。
                        return new TwinListBoxViewModel<MyQuery>(queries, model.Items);
                    }).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
