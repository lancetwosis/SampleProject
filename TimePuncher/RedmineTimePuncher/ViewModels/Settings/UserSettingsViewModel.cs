using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
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
                        error2.Value = "ユーザー情報取得中";
                        users.Value = await Task.Run(() => r.Users.Value.Where(u => u.Id != r.MyUser.Id).ToList());
                        error2.Value = null;
                    }
                    catch (Exception ex)
                    {
                        error2.Value = ex.Message;
                    }
                }
                else 
                {
                    error2.Value = "システム管理者APIキーが未設定です。";
                }
            }).AddTo(disposables);

            users.Where(a => a != null).Merge(ImportCommand).SubscribeWithErr(_ => setUpItemsSource(model, users.Value)).AddTo(disposables);
        }

        private void setUpItemsSource(UserSettingsModel model, List<MyUser> users)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable();

            model.Items = new ObservableCollection<MyUser>(model.Items.Select(a => users.FirstOrDefault(b => a.Id == b.Id)).Where(a => a != null));
            var selectedItems = new ObservableCollection<MyUser>(model.Items);
            TwinListBoxViewModel = new TwinListBoxViewModel<MyUser>(users, selectedItems);
            selectedItems.ObserveAddChanged().SubscribeWithErr(a => model.Items.Add(a)).AddTo(myDisposables);
            selectedItems.ObserveRemoveChanged().SubscribeWithErr(a => model.Items.Remove(a)).AddTo(myDisposables);
            selectedItems.CollectionChangedAsObservable().Where(a => a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move).SubscribeWithErr(a =>
            {
                model.Items.Move(a.OldStartingIndex, a.NewStartingIndex);
            }).AddTo(myDisposables);
        }
    }
}
