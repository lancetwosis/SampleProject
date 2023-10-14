using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public abstract class FilterGroupViewModelBase : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        public SolidColorBrush Background { get; set; }
        public ReadOnlyReactivePropertySlim<string> Label { get; set; }
        public ReadOnlyReactivePropertySlim<string> Tooltip { get; set; }

        protected string NAN = "NaN";

        public FilterGroupViewModelBase(ReactivePropertySlim<bool> isEnabled, SolidColorBrush background)
        {
            IsEnabled = isEnabled.AddTo(disposables);
            Background = background;
        }
    }
}
