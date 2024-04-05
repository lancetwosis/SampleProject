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

        public AppointmentSettingsViewModel(AppointmentSettingsModel model, ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(model)
        {
            MyWorks = model.ObserveProperty(a => a.MyWorks).Select(a => new AppointmentMyWorksSettingsViewModel(a)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Redmine = model.ObserveProperty(a => a.Redmine).Select(a => new AppointmentRedmineSettingsViewModel(a, redmine, errorMessage)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Outlook = model.ObserveProperty(a => a.Outlook).Select(a => new AppointmentOutlookSettingsViewModel(a)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Teams = model.ObserveProperty(a => a.Teams).Select(a => new AppointmentTeamsSettingsViewModel(a)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
