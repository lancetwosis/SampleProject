using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class PersonHourReportSettingsModel : Bases.SettingsModelBase<PersonHourReportSettingsModel>
    {
        public ObservableCollection<PersonHourReportSettingModel> Items { get; set; }

        public PersonHourReportSettingsModel()
        {
            Items = new ObservableCollection<PersonHourReportSettingModel>()
            {
                new PersonHourReportSettingModel() { Period = ReportPeriodType.ThisWeek },
                new PersonHourReportSettingModel() { Period = ReportPeriodType.ThisMonth },
            };
        }
    }
}
