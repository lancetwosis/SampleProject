using LibRedminePower.Extentions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class CalendarSettingsModel : Bases.SettingsModelBase<CalendarSettingsModel>
    {
        public WorkingDaySettingModel Sun { get; set; }
        public WorkingDaySettingModel Mon { get; set; }
        public WorkingDaySettingModel Tue { get; set; }
        public WorkingDaySettingModel Wed { get; set; }
        public WorkingDaySettingModel Thu { get; set; }
        public WorkingDaySettingModel Fri { get; set; }
        public WorkingDaySettingModel Sat { get; set; }

        public ObservableCollection<DateTime> SpecialDates { get; set; }

        public PersonalHolidaySettingModel OffTimeFromCatetories { get; set; }
        public PersonalHolidaySettingModel OffTimeFromSubject { get; set; }

        public CalendarSettingsModel()
        {
            Sun = new WorkingDaySettingModel(DayOfWeek.Sunday,    false);
            Mon = new WorkingDaySettingModel(DayOfWeek.Monday,    true);
            Tue = new WorkingDaySettingModel(DayOfWeek.Tuesday,   true);
            Wed = new WorkingDaySettingModel(DayOfWeek.Wednesday, true);
            Thu = new WorkingDaySettingModel(DayOfWeek.Thursday,  true);
            Fri = new WorkingDaySettingModel(DayOfWeek.Friday,    true);
            Sat = new WorkingDaySettingModel(DayOfWeek.Saturday,  false);

            SpecialDates = new ObservableCollection<DateTime>();

            OffTimeFromSubject = new PersonalHolidaySettingModel();
            OffTimeFromCatetories = new PersonalHolidaySettingModel();
        }

        public bool IsWorkingDay(DateTime date)
        {
            var daysOfWeek = new[] { Sun, Mon, Tue, Wed, Thu, Fri, Sat };

            var special = SpecialDates.FirstOrDefault(d => d == date);
            if (special != default(DateTime))
                return !daysOfWeek.First(w => w.DayOfWeek == special.DayOfWeek).IsWorkingDay;
            else
                return daysOfWeek.First(w => w.DayOfWeek == date.DayOfWeek).IsWorkingDay;
        }

        public bool IsOnTimeAppointment(MyAppointment appo)
        {
            return !OffTimeFromSubject.IsMatch(appo.Subject) && !OffTimeFromCatetories.IsMatch(appo.OutlookCategories);
        }
    }

    public class WorkingDaySettingModel : LibRedminePower.Models.Bases.ModelBase
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }

        public WorkingDaySettingModel()
        { }

        public WorkingDaySettingModel(DayOfWeek dayOfWeek, bool isWorkingDay)
        {
            DayOfWeek = dayOfWeek;
            IsWorkingDay = isWorkingDay;
        }
    }

    public class PersonalHolidaySettingModel : LibRedminePower.Models.Bases.ModelBase
    {
        public bool IsEnabled { get; set; }
        public string Pattern { get; set; }

        public PersonalHolidaySettingModel()
        {
        }

        public bool IsMatch(string input)
        {
            return IsEnabled && input != null && Pattern != null ? Regex.IsMatch(input, Pattern) : false;
        }
    }
}
