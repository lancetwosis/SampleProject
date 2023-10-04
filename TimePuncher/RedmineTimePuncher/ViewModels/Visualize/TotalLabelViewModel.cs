using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class TotalLabelViewModel : FactorTypeViewModel
    {
        public ReadOnlyReactivePropertySlim<double> TotalHours { get; set; }
        public ReadOnlyReactivePropertySlim<Visibility> IsVisible { get;  set; }
        public ReadOnlyReactivePropertySlim<HorizontalAlignment> HorizonAlign { get; set; }
        public ReadOnlyReactivePropertySlim<VerticalAlignment> VerticalAlign { get; set; }

        public TotalLabelViewModel(ReadOnlyReactivePropertySlim<bool> isEnabled, ReactivePropertySlim<FactorType> selectedType)
            : base("合計", isEnabled, selectedType, FactorTypes.None, FactorTypes.Center, FactorTypes.TopLeft, FactorTypes.TopRight, FactorTypes.BottomLeft, FactorTypes.BottomRight)
        {
            IsVisible = SelectedType.Select(t =>
            {
                switch (t.ValueType)
                {
                    case FactorValueType.None:
                        return Visibility.Collapsed;
                    case FactorValueType.Center:
                    case FactorValueType.TopLeft:
                    case FactorValueType.BottomLeft:
                    case FactorValueType.TopRight:
                    case FactorValueType.BottomRight:
                        return Visibility.Visible;
                    default:
                        throw new InvalidOperationException();
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            HorizonAlign = SelectedType.Select(t =>
            {
                switch (t.ValueType)
                {
                    case FactorValueType.None:
                    case FactorValueType.Center:
                        return HorizontalAlignment.Center;
                    case FactorValueType.TopLeft:
                    case FactorValueType.BottomLeft:
                        return HorizontalAlignment.Left;
                    case FactorValueType.TopRight:
                    case FactorValueType.BottomRight:
                        return HorizontalAlignment.Right;
                    default:
                        throw new InvalidOperationException();
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            VerticalAlign = SelectedType.Select(t =>
            {
                switch (t.ValueType)
                {
                    case FactorValueType.None:
                    case FactorValueType.Center:
                        return VerticalAlignment.Center;
                    case FactorValueType.TopLeft:
                    case FactorValueType.TopRight:
                        return VerticalAlignment.Top;
                    case FactorValueType.BottomLeft:
                    case FactorValueType.BottomRight:
                        return VerticalAlignment.Bottom;
                    default:
                        throw new InvalidOperationException();
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
