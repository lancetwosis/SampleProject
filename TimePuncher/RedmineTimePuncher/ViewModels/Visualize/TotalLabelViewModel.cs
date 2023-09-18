using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
            : base("合計", isEnabled, selectedType, FactorType.None, FactorType.Center, FactorType.TopLeft, FactorType.TopRight, FactorType.BottomLeft, FactorType.BottomRight)
        {
            IsVisible = SelectedType.Select(t =>
            {
                switch (t.ValueType)
                {
                    case Enums.FactorValueType.None:
                        return Visibility.Collapsed;
                    case Enums.FactorValueType.Center:
                    case Enums.FactorValueType.TopLeft:
                    case Enums.FactorValueType.BottomLeft:
                    case Enums.FactorValueType.TopRight:
                    case Enums.FactorValueType.BottomRight:
                        return Visibility.Visible;
                    default:
                        throw new InvalidOperationException();
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            HorizonAlign = SelectedType.Select(t =>
            {
                switch (t.ValueType)
                {
                    case Enums.FactorValueType.None:
                    case Enums.FactorValueType.Center:
                        return HorizontalAlignment.Center;
                    case Enums.FactorValueType.TopLeft:
                    case Enums.FactorValueType.BottomLeft:
                        return HorizontalAlignment.Left;
                    case Enums.FactorValueType.TopRight:
                    case Enums.FactorValueType.BottomRight:
                        return HorizontalAlignment.Right;
                    default:
                        throw new InvalidOperationException();
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            VerticalAlign = SelectedType.Select(t =>
            {
                switch (t.ValueType)
                {
                    case Enums.FactorValueType.None:
                    case Enums.FactorValueType.Center:
                        return VerticalAlignment.Center;
                    case Enums.FactorValueType.TopLeft:
                    case Enums.FactorValueType.TopRight:
                        return VerticalAlignment.Top;
                    case Enums.FactorValueType.BottomLeft:
                    case Enums.FactorValueType.BottomRight:
                        return VerticalAlignment.Bottom;
                    default:
                        throw new InvalidOperationException();
                }
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
