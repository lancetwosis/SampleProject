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

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class FactorTypeViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string Title { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsEnabled { get; set; }

        public ReactivePropertySlim<FactorType> SelectedType { get; set; }
        public ObservableCollection<FactorType> Types { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsEdited { get; set; }

        public ReadOnlyReactivePropertySlim<bool> IsContinuous { get; set; }

        private ReactivePropertySlim<FactorType> previousType { get; set; }

        public FactorTypeViewModel(string title, ReadOnlyReactivePropertySlim<bool> isEnabled, ReactivePropertySlim<FactorType> selectedType, params FactorType[] types)
        {
            Title = title;
            IsEnabled = isEnabled;

            Types = new ObservableCollection<FactorType>(types);
            SelectedType = selectedType.AddTo(disposables);
            previousType = new ReactivePropertySlim<FactorType>(SelectedType.Value).AddTo(disposables);
            IsEdited = SelectedType.CombineLatest(previousType, (v, pre) => v != pre).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsContinuous = SelectedType.Select(t => t == FactorType.Date).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void UpdatePreviousType()
        {
            previousType.Value = SelectedType.Value;
        }
    }
}
