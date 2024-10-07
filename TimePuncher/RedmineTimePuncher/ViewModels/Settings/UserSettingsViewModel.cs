using LibRedminePower.Extentions;
using LibRedminePower.Models;
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
    public class UserSettingsViewModel : Bases.SettingsViewModelBase<UserSettingsModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReadOnlyReactivePropertySlim<TwinListBoxViewModel<MyUser>> TwinListBoxViewModel { get; set; }

        public UserSettingsViewModel(UserSettingsModel model) :base(model)
        {
            ErrorMessage =
                CacheTempManager.Default.Message.CombineLatest(
                    CacheTempManager.Default.CanUseAdminApiKey, (e, c) => 
                    !string.IsNullOrEmpty(e) ? e : (!c ? Resources.RedmineMngMsgAdminAPIKeyNotSet : null)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            TwinListBoxViewModel =
                // ModelのItemsが更新されたら（インポートなどでも）
                model.ObserveProperty(a => a.Items).CombineLatest(
                    // かつ、Cacheが更新されたら
                    CacheTempManager.Default.MyOtherUsers.Where(a => a != null),
                    (items, users) => {
                        // まずは、RedmineのUsersで削除された物は、ModelのItemsから削除する。
                        model.UpdateItems(users);
                        // 次に、更新したModelのItemsから、TiwnListBoxのViewModelを作る。
                        return new TwinListBoxViewModel<MyUser>(users, model.Items);
                    }).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

    }
}
