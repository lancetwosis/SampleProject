using LibRedminePower.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class UserSettingsModel : Bases.SettingsModelBase<UserSettingsModel>
    {
        public ObservableCollection<MyUser> Items { get; set; } = new ObservableCollection<MyUser>();
    }
}
