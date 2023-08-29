using RedmineTimePuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.Bases
{
    public abstract class AppointmentResouceSettingsModelBase : LibRedminePower.Models.Bases.ModelBase, IAutoUpdateSetting
    {
        public bool IsAutoUpdate { get; set; } = false;
        public int AutoUpdateMinutes { get; set; } = 15;
    }
}
