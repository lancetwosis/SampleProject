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
        public ReadOnlyReactivePropertySlim<List<Tracker>> Trackers { get; set; }

        public ReactivePropertySlim<bool> IsIgnoreTrackers { get; set; }
        public ObservableCollectionSync<Tracker, Tracker> IgnoreTrackers { get; set; }
        public ReactivePropertySlim<bool> IsIgnoreText { get; set; }
        public ReactivePropertySlim<string> IgnoreText { get; set; }

        public AppointmentRedmineSettingsViewModel(AppointmentRedmineSettingsModel model) : base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Trackers = CacheTempManager.Default.Trackers.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsIgnoreTrackers = model.ToReactivePropertySlimAsSynchronized(a => a.IsIgnoreTrackers).AddTo(disposables);
            IgnoreTrackers = new ObservableCollectionSync<Tracker, Tracker>(model.IgnoreTrackers, a => a, a => a).AddTo(disposables);
            IsIgnoreText = model.ToReactivePropertySlimAsSynchronized(a => a.IsIgnoreText).AddTo(disposables);
            IgnoreText = model.ToReactivePropertySlimAsSynchronized(a => a.IgnoreText).AddTo(disposables);
        }
    }
}
