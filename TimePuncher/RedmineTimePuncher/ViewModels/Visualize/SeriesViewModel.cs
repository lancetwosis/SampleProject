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
        public string ToolTip { get; set; }
        public string Url { get; set; }

        public ObservableCollection<PointViewModel> Points { get; set; }

        public ReactivePropertySlim<bool> IsVisible { get; set; }
        public ReactiveCommand VisibleAllCommand { get; set; }
        public ReactiveCommand InvisibleAllCommand { get; set; }

        public FactorModel Factor { get; set; }

        private SeriesViewModel(ViewType type, string title, FactorModel factor, ReactiveCommand visibleAll, ReactiveCommand invisibleAll)
        {
            Type = type;
            Points = new ObservableCollection<PointViewModel>();
            Title = title;
            Factor = factor;
            Color = Factor != null ? Factor.GetColor() : FactorType.None.GetColor(Title);
            if (Factor != null && Factor.Type == FactorType.Issue)
            {
                ToolTip = (Factor.RawValue as Issue).GetFullLabel();
                Url = MyIssue.GetUrl((Factor.RawValue as Issue).Id);
            }
            else if (Factor != null && Factor.Type == FactorType.User)
            {
                ToolTip = Title;
                Url = MyUser.GetUrl((Factor.RawValue as IdentifiableName).Id);
            }
            else
            {
                ToolTip = Title;
            }

            IsVisible = new ReactivePropertySlim<bool>(true).AddTo(disposables);
            VisibleAllCommand = visibleAll;
            InvisibleAllCommand = invisibleAll;
        }

        public SeriesViewModel(ViewType type) : this(type, "工数", null, null, null)
        {
        }

        public SeriesViewModel(ViewType type, FactorModel factor, ReactiveCommand visibleAll, ReactiveCommand invisibleAll)
            : this(type, factor.Name, factor, visibleAll, invisibleAll)
        {
        }
    }
}
