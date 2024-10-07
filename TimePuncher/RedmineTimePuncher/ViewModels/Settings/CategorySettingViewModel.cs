using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using RedmineTimePuncher.Views.Settings;
using Telerik.Windows.Controls;
using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RedmineTimePuncher.Models.Managers;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class CategorySettingViewModel : LibRedminePower.ViewModels.Bases.ColorSettingViewModelBase
    {
        public string Name { get; set; }

        public ReadOnlyReactivePropertySlim<SolidColorBrush> ForeColor { get; set; }
        public ReadOnlyReactivePropertySlim<SolidColorBrush> BackColor { get; set; }
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<bool> IsBold { get; set; }
        public ReactivePropertySlim<bool> IsItalic { get; set; }
        public ReactivePropertySlim<Enums.AndOrType> AndOrType { get; set; }
        public ObservableCollection<MyTracker> TargetTrackers { get; set; }
        public EditableGridViewModel<AssignRuleViewModel, AssignRuleModel> Rules { get; set; }
        public ReadOnlyReactivePropertySlim<string> Details { get; set; }
        public ReactivePropertySlim<bool> IsWorkingTime { get; set; }

        // CategorySettingsView にて Resources に登録するために必要
        public CategorySettingViewModel()
        {
        }

        private CategorySettingModel model;

        public CategorySettingViewModel(CategorySettingModel model)
        {
            this.model = model;

            Name = model.TimeEntry.Name;
            IsEnabled = model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables);
            IsBold = model.ToReactivePropertySlimAsSynchronized(a => a.IsBold).AddTo(disposables);
            IsItalic = model.ToReactivePropertySlimAsSynchronized(a => a.IsItalic).AddTo(disposables);
            AndOrType = model.ToReactivePropertySlimAsSynchronized(a => a.AndOrType).AddTo(disposables);

            model.TargetTrackers = CacheTempManager.Default.MyTrackers.Value.Where(t => model.TargetTrackers.Contains(t)).ToList();
            TargetTrackers = new ObservableCollection<MyTracker>(model.TargetTrackers);
            TargetTrackers.ObserveAddChanged().SubscribeWithErr(a => model.TargetTrackers.Add(a)).AddTo(disposables);
            TargetTrackers.ObserveRemoveChanged().SubscribeWithErr(a => model.TargetTrackers.Remove(a)).AddTo(disposables);

            Color = model.ToReactivePropertySlimAsSynchronized(a => a.Color, a => a.ToMediaColor(), a => a.ToDrawingColor()).AddTo(disposables);
            var foreColor = model.ToReactivePropertySlimAsSynchronized(a => a.ForeColor, a => a.ToMediaColor(), a => a.ToDrawingColor()).AddTo(disposables);
            BackColor = Color.Select(a => new SolidColorBrush(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            ForeColor = foreColor.Select(a => new SolidColorBrush(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Rules = new EditableGridViewModel<AssignRuleViewModel, AssignRuleModel>(model.Rules, a => new AssignRuleViewModel(a), a => a.Model);

            Details =
                Rules.CollectionChangedAsObservable().StartWithDefault().CombineLatest
                (Rules.ObserveElementObservableProperty(a => a.Detail), AndOrType,
                (_, __, ___) =>
                {
                    if (Rules.Count == 1)
                        return $"[{Rules.First().Detail.Value}]";
                    else
                        return string.Join(Environment.NewLine, Rules.Indexed().Select(a =>
                        {
                            var result = $"[{a.v.Detail.Value}]";
                            if (!a.isLast) result = result + " " + AndOrType.Value.GetDescription();
                            return result;
                        }));
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsWorkingTime = model.ToReactivePropertySlimAsSynchronized(a => a.IsWorkingTime).AddTo(disposables);
        }

        public void SetOrder(int order)
        {
            model.Order = order;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}