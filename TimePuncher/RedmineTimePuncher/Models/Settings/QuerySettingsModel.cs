using RedmineTimePuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class QuerySettingsModel : Bases.SettingsModelBase<QuerySettingsModel>, IAutoUpdateSetting
    {
        public bool IsAutoUpdate { get; set; } = false;
        public int AutoUpdateMinutes { get; set; } = 15;
        public ObservableCollection<MyQuery> Items { get; set; } = new ObservableCollection<MyQuery>();
    }
}
