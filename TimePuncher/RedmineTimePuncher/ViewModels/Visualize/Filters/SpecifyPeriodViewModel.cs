using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Filters;
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
    public class SpecifyPeriodViewModel : FilterGroupViewModelBase
    {
        public ReactivePropertySlim<PeriodType> PeriodMode { get; private set; }
        public ReactivePropertySlim<DateTime> Start { get; set; }
        public ReactivePropertySlim<DateTime> End { get; set; }

        // https://www.colordic.org/colorsample/f7f0ff
        public SpecifyPeriodViewModel(TicketFiltersModel model, DateTime? createAt)
            : base(model.ToReactivePropertySlimAsSynchronized(a => a.SpecifyPeriod), ColorEx.ToBrush("#f7f0ff"))
        {
            PeriodMode = model.ToReactivePropertySlimAsSynchronized(a => a.PeriodMode).AddTo(disposables);
            Start = model.ToReactivePropertySlimAsSynchronized(a => a.Start).AddTo(disposables);
            End = model.ToReactivePropertySlimAsSynchronized(a => a.End).AddTo(disposables);

            IsValid = PeriodMode.CombineLatest(Start, End, (_1, _2, _3) => true).Select(_ =>
            {
                if (PeriodMode.Value.IsRelative())
                    return null;

                if (Start.Value > End.Value)
                    return Resources.VisualizePeriodErrMsg;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Label = IsEnabled.CombineLatest(IsValid, PeriodMode, Start, End, (_1, _2, _3, _4, _5) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return $"{Resources.VisualizePeriod}: {Resources.VisualizeNotSpecified}";
                else if (IsValid.Value != null)
                    return $"{Resources.VisualizePeriod}: {NAN}";

                if (createAt.HasValue)
                {
                    var start = model.GetStart(createAt.Value).ToString("yy/MM/dd");
                    var end = model.GetEnd(createAt.Value).ToString("yy/MM/dd");
                    switch (PeriodMode.Value)
                    {
                        case PeriodType.LastWeek:
                            return $"{Resources.VisualizePeriodLastWeek} ({start} - {end})";
                        case PeriodType.LastMonth:
                            return $"{Resources.VisualizePeriodLastMonth} ({start} - {end})";
                        case PeriodType.SpecifyPeriod:
                            return $"{start} - {end}";
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    switch (PeriodMode.Value)
                    {
                        case PeriodType.LastWeek:
                            return Resources.VisualizePeriodLastWeek;
                        case PeriodType.LastMonth:
                            return Resources.VisualizePeriodLastMonth;
                        case PeriodType.SpecifyPeriod:
                            var start = model.GetStart(DateTime.Today).ToString("yy/MM/dd");
                            var end = model.GetEnd(DateTime.Today).ToString("yy/MM/dd");
                            return $"{start} - {end}";
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
