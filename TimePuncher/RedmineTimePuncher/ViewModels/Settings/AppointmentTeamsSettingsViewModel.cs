using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.ViewModels.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class AppointmentTeamsSettingsViewModel : AppointmentResouceSettingsViewModelBase
    {
        public NeedsRestartSettingViewModel<bool> IsEnabled { get; set; }
        public NeedsRestartSettingViewModel<bool> IsEnabledCallHistory { get; set; }
        public NeedsRestartSettingViewModel<bool> IsEnabledStatus { get; set; }

        public AppointmentTeamsSettingsViewModel(AppointmentTeamsSettingsModel model) :base(model)
        {
            IsEnabled = new NeedsRestartSettingViewModel<bool>(model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables)).AddTo(disposables);
            IsEnabledCallHistory = new NeedsRestartSettingViewModel<bool>(model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabledCallHistory).AddTo(disposables)).AddTo(disposables);
            IsEnabledStatus = new NeedsRestartSettingViewModel<bool>(model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabledStatus).AddTo(disposables)).AddTo(disposables);
        }
    }
}
