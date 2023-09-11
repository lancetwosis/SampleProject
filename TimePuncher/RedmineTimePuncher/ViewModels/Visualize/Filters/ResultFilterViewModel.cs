using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ObservableCollectionSync;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public class ResultFilterViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }

        public ReactivePropertySlim<FactorType> Type { get; set; }
        public List<FactorType> AllTypes { get; set; }
        public ObservableCollection<FactorModel> Factors { get; set; }
        public ObservableCollection<FactorModel> AllFactors { get; set; }

        public ReactivePropertySlim<FilterType> FilterType { get; set; }

        public ResultFilterModel Model { get; set; }

        public ReadOnlyReactivePropertySlim<string> Label { get; set; }
        public ReadOnlyReactivePropertySlim<string> ShortLabel { get; set; }

        public ReactivePropertySlim<bool> NowEditing { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsValid { get; set; }

        public ReactiveCommand EditCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public ResultFilterViewModel(ResultFiltersViewModel parent, ResultFilterModel model)
        {
            Model = model;

            IsEnabled = model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);

            Type = model.ToReactivePropertySlimAsSynchronized(m => m.Type).AddTo(disposables);
            AllTypes = parent.AllTypes;

            AllFactors = getAllFactors(parent, model.Type);
            Factors = model.Factors;
            Type.Skip(1).Subscribe(t =>
            {
                Factors.Clear();
                AllFactors = getAllFactors(parent, t);
            }).AddTo(disposables);

            FilterType = model.ToReactivePropertySlimAsSynchronized(m => m.FilterType).AddTo(disposables);

            Label = IsEnabled.CombineLatest(
                Type,
                Factors.CollectionChangedAsObservable().StartWithDefault(),
                FilterType, (_1, _2, _3, _4) => true).Select(_ =>
                {
                    var equals = FilterType.Value == Models.Visualize.FilterType.Equals ? "==" : "!=";
                    return $"{Type.Value} {equals} {string.Join(", ", Factors.Select(f => f.Name))}";
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            ShortLabel = Label.Select(label => label.Count() > 31 ? $"{label.Substring(0, 27)}..." : label).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            NowEditing = new ReactivePropertySlim<bool>().AddTo(disposables);
            IsValid = NowEditing.Select(n => n ? false : Factors.Any()).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            EditCommand = new ReactiveCommand().WithSubscribe(() => NowEditing.Value = true).AddTo(disposables);
            DeleteCommand = new ReactiveCommand().WithSubscribe(() => parent.Items.Remove(this)).AddTo(disposables);
        }

        private ObservableCollection<FactorModel> getAllFactors(ResultFiltersViewModel parent, FactorType type)
        {
            switch (type)
            {
                case FactorType.Project:
                    return new ObservableCollection<FactorModel>(parent.Projects);
                case FactorType.User:
                    return new ObservableCollection<FactorModel>(parent.Users);
                case FactorType.Category:
                    return new ObservableCollection<FactorModel>(parent.Categories);
                case FactorType.Date:
                    return new ObservableCollection<FactorModel>(parent.Dates);
                case FactorType.OnTime:
                    return new ObservableCollection<FactorModel>(parent.OnTimes);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
