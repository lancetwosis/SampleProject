using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Resources
{
    public class ResourceSettingViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public ReadOnlyReactivePropertySlim<bool> IsLastEnabled { get; set; }
        public ReadOnlyReactivePropertySlim<string> ToolTip { get; set; }

        public MyResourceBase Resource { get; set; }

        public ResourceSettingViewModel(MyResourceBase resource)
        {
            Resource = resource;
            resource.ResourceSetting = this;

            DisplayName = resource.DisplayName;
            IsEnabled = !Properties.Settings.Default.ResourcesUnVisibles.Cast<string>().Any(b => resource.DisplayName == b);
        }

        public void SetIsLastEnabled(ReadOnlyReactiveCollection<ResourceSettingViewModel> parent)
        {
            if (IsLastEnabled != null) return;

            IsLastEnabled = parent.ObserveElementProperty(a => a.IsEnabled).Select(_ => parent.Where(r => r.IsEnabled).Count() == 1)
                .CombineLatest(this.ObserveProperty(a => a.IsEnabled), (last, enable) => last && enable).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            ToolTip = IsLastEnabled.Select(i => i ? Properties.Resources.msgErrSelectOneOrMore : null).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
