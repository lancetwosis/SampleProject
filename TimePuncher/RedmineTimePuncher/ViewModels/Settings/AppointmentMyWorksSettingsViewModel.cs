using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.ViewModels.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class AppointmentMyWorksSettingsViewModel : AppointmentResouceSettingsViewModelBase
    {
        public AppointmentMyWorksSettingsViewModel(AppointmentMyWorksSettingsModel model) :base(model)
        {
        }
    }
}
