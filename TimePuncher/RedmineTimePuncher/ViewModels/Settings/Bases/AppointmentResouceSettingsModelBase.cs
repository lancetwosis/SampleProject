using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.Bases
{
    public class AppointmentResouceSettingsViewModelBase : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsAutoUpdate { get; set; }
        public ReactivePropertySlim<int> AutoUpdateMinutes { get; set; }

        public AppointmentResouceSettingsViewModelBase(AppointmentResouceSettingsModelBase model)
        {
            IsAutoUpdate = model.ToReactivePropertySlimAsSynchronized(a => a.IsAutoUpdate).AddTo(disposables);
            AutoUpdateMinutes = model.ToReactivePropertySlimAsSynchronized(a => a.AutoUpdateMinutes).AddTo(disposables);
        }
    }
}
