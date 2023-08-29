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
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls.ChartView;

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class SeriesViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ViewType Type { get; set; }
        public string Title { get; set; }
        public Brush Color { get; set; }
        public string TootTip { get; set; }
        public string Url { get; set; }

        public ObservableCollection<PointViewModel> Points { get; set; }

        public ReactivePropertySlim<bool> IsVisble { get; set; }
        public ReactiveCommand<BarSeries> SwitchVisibilityCommand { get; set; }

        public FactorModel Factor { get; set; }

        private SeriesViewModel(ViewType type, string title, FactorModel factor)
        {
            Type = type;
            Points = new ObservableCollection<PointViewModel>();
            Title = title;
            Factor = factor;
            Color = Factor != null ? Factor.GetColor() : FactorType.None.GetColor(Title);
            if (Factor != null && Factor.Type == FactorType.Issue)
            {
                TootTip = (Factor.RawValue as Issue).GetFullLabel();
                Url = MyIssue.GetUrl((Factor.RawValue as Issue).Id);
            }

            IsVisble = new ReactivePropertySlim<bool>(true).AddTo(disposables);
            SwitchVisibilityCommand = new ReactiveCommand<BarSeries>().WithSubscribe(s =>
            {
                if (s.Visibility == System.Windows.Visibility.Visible)
                {
                    s.Visibility = System.Windows.Visibility.Collapsed;
                    IsVisble.Value = false;
                }
                else
                {
                    s.Visibility = System.Windows.Visibility.Visible;
                    IsVisble.Value = true;
                }
            }).AddTo(disposables);
        }

        public SeriesViewModel(ViewType type) : this(type, "工数", null)
        {
        }

        public SeriesViewModel(ViewType type, FactorModel factor) : this(type, factor.Name, factor)
        {
        }
    }
}
