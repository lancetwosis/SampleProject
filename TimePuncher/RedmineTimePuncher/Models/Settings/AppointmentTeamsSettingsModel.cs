using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class AppointmentTeamsSettingsModel : Bases.AppointmentResouceSettingsModelBase
    {
        public bool IsEnabled { get; set; } = true;

        public AppointmentTeamsSettingsModel()
        {
            IsAutoUpdate = true;
            AutoUpdateMinutes = 30;
        }
    }
}
