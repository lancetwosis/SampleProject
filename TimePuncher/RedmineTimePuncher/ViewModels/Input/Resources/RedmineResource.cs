using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
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
    public class RedmineResource : Bases.MyResourceBase
    {
        public ReadOnlyReactivePropertySlim<string> Url { get; set; }

        public RedmineResource(ReadOnlyReactivePropertySlim<string> urlBase, ReactivePropertySlim<RedmineManager> redmine)
            : base(Bases.Type.Redmine, Properties.Resources.ResourceNameRedmineActivity, Properties.Resources.redmine16, Colors.Crimson, true)
        {
            Updater.Indicator.ToolTip = string.Format(Properties.Resources.LastUpdateTime, Properties.Resources.ResourceNameRedmineActivity);
            Updater.Indicator.DateTime = Properties.Settings.Default.LastTimeIndicatorRedmine;
            Url = urlBase.CombineLatest(redmine, (u, r) => u + ((r == null) ? u : $"activity?user_id={CacheManager.Default.MyUser.Id}")).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
