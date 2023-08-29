using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class AppointmentMyWorksSettingsModel : Bases.AppointmentResouceSettingsModelBase
    {
        public AppointmentMyWorksSettingsModel() :base()
        {
            IsAutoUpdate = true;
            AutoUpdateMinutes = 2;
        }
    }
}
