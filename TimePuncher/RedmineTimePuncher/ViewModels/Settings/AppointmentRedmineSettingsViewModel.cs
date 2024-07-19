using LibRedminePower.Extentions;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class AppointmentRedmineSettingsViewModel : AppointmentResouceIgnoreSettingsViewModelBase
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public List<Tracker> Trackers { get; set; }
        public ReactivePropertySlim<bool> IsIgnoreTrackers { get; set; }
        public ObservableCollectionSync<Tracker, Tracker> IgnoreTrackers { get; set; }
        public ReactivePropertySlim<bool> IsIgnoreText { get; set; }
        public ReactivePropertySlim<string> IgnoreText { get; set; }

        public AppointmentRedmineSettingsViewModel(AppointmentRedmineSettingsModel model, ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(model)
        {
            var error1 = errorMessage.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var error2 = new ReactivePropertySlim<string>().AddTo(disposables);
            ErrorMessage = new IObservable<string>[] { error1, error2 }.CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsIgnoreTrackers = model.ToReactivePropertySlimAsSynchronized(a => a.IsIgnoreTrackers).AddTo(disposables);
            IgnoreTrackers = new ObservableCollectionSync<Tracker, Tracker>(model.IgnoreTrackers, a => a, a => a).AddTo(disposables);
            IsIgnoreText = model.ToReactivePropertySlimAsSynchronized(a => a.IsIgnoreText).AddTo(disposables);
            IgnoreText = model.ToReactivePropertySlimAsSynchronized(a => a.IgnoreText).AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                try
                {
                    error2.Value = Resources.SettingsMsgNowGettingData;
                    Trackers = CacheManager.Default.TmpTrackers;
                    error2.Value = null;
                }
                catch (Exception ex)
                {
                    error2.Value = ex.Message;
                }
            }).AddTo(disposables);
        }
    }
}
