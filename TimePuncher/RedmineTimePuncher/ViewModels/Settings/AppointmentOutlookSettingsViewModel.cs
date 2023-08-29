using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class AppointmentOutlookSettingsViewModel : AppointmentResouceIgnoreSettingsViewModelBase
    {
        public NeedsRestartSettingViewModel<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<string> RefsKeywords { get; set; }
        public ReactivePropertySlim<bool> IsReflectLastInput { get; set; }

        public AppointmentOutlookSettingsViewModel(AppointmentOutlookSettingsModel model) : base(model)
        {
            IsEnabled = new NeedsRestartSettingViewModel<bool>(model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables)).AddTo(disposables);
            RefsKeywords = model.ToReactivePropertySlimAsSynchronized(a => a.RefsKeywords).AddTo(disposables);
            IsReflectLastInput = model.ToReactivePropertySlimAsSynchronized(a => a.IsReflectLastInput).AddTo(disposables);
        }
    }
}
