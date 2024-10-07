using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class CalendarSettingsViewModel : Bases.SettingsViewModelBase<CalendarSettingsModel>
    {
        public ReactivePropertySlim<bool> Sun { get; set; }
        public ReactivePropertySlim<bool> Mon { get; set; }
        public ReactivePropertySlim<bool> Tue { get; set; }
        public ReactivePropertySlim<bool> Wed { get; set; }
        public ReactivePropertySlim<bool> Thu { get; set; }
        public ReactivePropertySlim<bool> Fri { get; set; }
        public ReactivePropertySlim<bool> Sat { get; set; }

        public DateTime DisplayDate { get; set; }

        public ReadOnlyReactivePropertySlim<PersonalHolidaySettingViewModel> UseSubject { get; set; }
        public ReadOnlyReactivePropertySlim<PersonalHolidaySettingViewModel> UseCategory { get; set; }
        public ReadOnlyReactivePropertySlim<ObservableCollection<DateTime>> SpecialDates { get; set; }


        public CalendarSettingsViewModel(CalendarSettingsModel model) : base(model)
        {
            Sun = model.ToReactivePropertySlimAsSynchronized(m => m.Sun.IsWorkingDay).AddTo(disposables);
            Mon = model.ToReactivePropertySlimAsSynchronized(m => m.Mon.IsWorkingDay).AddTo(disposables);
            Tue = model.ToReactivePropertySlimAsSynchronized(m => m.Tue.IsWorkingDay).AddTo(disposables);
            Wed = model.ToReactivePropertySlimAsSynchronized(m => m.Wed.IsWorkingDay).AddTo(disposables);
            Thu = model.ToReactivePropertySlimAsSynchronized(m => m.Thu.IsWorkingDay).AddTo(disposables);
            Fri = model.ToReactivePropertySlimAsSynchronized(m => m.Fri.IsWorkingDay).AddTo(disposables);
            Sat = model.ToReactivePropertySlimAsSynchronized(m => m.Sat.IsWorkingDay).AddTo(disposables);

            UseSubject = model.ToReadOnlyViewModel(a => a.OffTimeFromSubject, a => new PersonalHolidaySettingViewModel(a)).AddTo(disposables);
            UseCategory = model.ToReadOnlyViewModel(a => a.OffTimeFromCatetories, a => new PersonalHolidaySettingViewModel(a)).AddTo(disposables);

            DisplayDate = new DateTime(DateTime.Today.Year, 1, 1);
            SpecialDates = model.ObserveProperty(a => a.SpecialDates).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }

    public class PersonalHolidaySettingViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<string> Pattern { get; set; }

        public PersonalHolidaySettingViewModel(PersonalHolidaySettingModel model)
        {
            IsEnabled = model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);
            Pattern = model.ToReactivePropertySlimAsSynchronized(m => m.Pattern).AddTo(disposables);
        }
    }
}
