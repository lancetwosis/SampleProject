using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Resources
{
    public class ResourceSettingViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; } = true;

        public MyResourceBase Resource { get; set; }

        public ResourceSettingViewModel(MyResourceBase resource)
        {
            Resource = resource;
            resource.ResourceSetting = this;

            DisplayName = resource.DisplayName;
            IsEnabled = !Properties.Settings.Default.ResourcesUnVisibles.Cast<string>().Any(b => resource.DisplayName == b);
        }
    }
}
