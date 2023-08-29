using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Resources
{
    public class MyWorksResource : Bases.MyResourceBase
    {
        public ReadOnlyReactivePropertySlim<string> Url { get; set; }

        public MyWorksResource(ReadOnlyReactivePropertySlim<string> urlBase)
            : base(Bases.Type.MyWorks, Properties.Resources.ResourceNameMyWork, Properties.Resources.work16, Colors.Green, false)
        {
            Url = urlBase.Select(u => u + "time_entries").ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Updater.Indicator.ToolTip = string.Format(Properties.Resources.LastUpdateTime, Properties.Resources.ResourceNameMyWork);
            Updater.Indicator.DateTime = Properties.Settings.Default.LastTimeIndicatorMyWorks;
        }
    }
}
