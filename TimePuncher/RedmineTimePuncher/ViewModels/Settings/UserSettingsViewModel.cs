using LibRedminePower.Extentions;
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
        public TwinListBoxViewModel<MyUser> TwinListBoxViewModel { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        private CompositeDisposable myDisposables;

        public UserSettingsViewModel(UserSettingsModel model, ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) :base(model)
        {
            var error1 = errorMessage.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var error2 = new ReactivePropertySlim<string>().AddTo(disposables);
            ErrorMessage = new IObservable<string>[] { error1, error2 }.CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            var users = new ReactivePropertySlim<List<MyUser>>().AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                if (r.CanUseAdminApiKey())
                {
                    try
                    {
                        error2.Value = Resources.SettingsMsgNowGettingData;

                        // ユーザの選択肢から自分自身は除外する
                        var myUsers = CacheManager.Default.GetTemporaryUsers();
                        var myUser = CacheManager.Default.GetTemporaryMyUser();
                        users.Value = myUsers.Where(u => u.Id != myUser.Id).ToList();

                        error2.Value = null;
                    }
                    catch (Exception ex)
                    {
                        error2.Value = ex.Message;
                    }
                }
                else
                {
                    error2.Value = Resources.RedmineMngMsgAdminAPIKeyNotSet;
                }
            }).AddTo(disposables);

            users.Where(a => a != null).Merge(ImportCommand).SubscribeWithErr(_ => setUpItemsSource(model, users.Value)).AddTo(disposables);
        }

        private void setUpItemsSource(UserSettingsModel model, List<MyUser> users)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            // 元の設定に新たな「ユーザの選択肢」に含まれないユーザがあった場合、除外する
            model.Items = new ObservableCollection<MyUser>(model.Items.Select(a => users.FirstOrDefault(b => a.Id == b.Id)).Where(a => a != null));
            TwinListBoxViewModel = new TwinListBoxViewModel<MyUser>(users, model.Items).AddTo(myDisposables);
        }
    }
}
