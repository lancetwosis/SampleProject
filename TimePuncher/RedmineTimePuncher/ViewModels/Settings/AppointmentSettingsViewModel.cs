using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
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
    public class AppointmentSettingsViewModel : Bases.SettingsViewModelBase<AppointmentSettingsModel>
    {
        public ReadOnlyReactivePropertySlim<AppointmentMyWorksSettingsViewModel> MyWorks { get; set; }
        public ReadOnlyReactivePropertySlim<AppointmentRedmineSettingsViewModel> Redmine { get; set; }
        public ReadOnlyReactivePropertySlim<AppointmentOutlookSettingsViewModel> Outlook { get; set; }
        public ReadOnlyReactivePropertySlim<AppointmentTeamsSettingsViewModel> Teams { get; set; }

        public AppointmentSettingsViewModel(AppointmentSettingsModel model) : base(model)
        {
            MyWorks = model.ToReadOnlyViewModel(a => a.MyWorks, a => new AppointmentMyWorksSettingsViewModel(a)).AddTo(disposables);
            Redmine = model.ToReadOnlyViewModel(a => a.Redmine, a => new AppointmentRedmineSettingsViewModel(a)).AddTo(disposables);
            Outlook = model.ToReadOnlyViewModel(a => a.Outlook, a => new AppointmentOutlookSettingsViewModel(a)).AddTo(disposables);
            Teams = model.ToReadOnlyViewModel(a => a.Teams, a => new AppointmentTeamsSettingsViewModel(a)).AddTo(disposables);
        }
    }
}
