using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.Bases
{
    public class AppointmentResouceIgnoreSettingsViewModelBase : AppointmentResouceSettingsViewModelBase
    {
        public ReactivePropertySlim<bool> IsIgnoreMinutes { get; set; }
        public ReactivePropertySlim<int> IgnoreMinutes { get; set; }

        public AppointmentResouceIgnoreSettingsViewModelBase(AppointmentResouceIgnoreSettingsModelBase model) : base(model)
        {
            IsIgnoreMinutes = model.ToReactivePropertySlimAsSynchronized(a => a.IsIgnoreMinutes).AddTo(disposables);
            IgnoreMinutes = model.ToReactivePropertySlimAsSynchronized(a => a.IgnoreMinutes).AddTo(disposables);
        }
    }
}
