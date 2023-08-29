using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Interfaces
{
    public interface IAutoUpdateSetting
    {
        bool IsAutoUpdate { get; set; }
        int AutoUpdateMinutes { get; set; }
    }
}
