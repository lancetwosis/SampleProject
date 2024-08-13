using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api.Types;
using System.Windows.Media;
using RedmineTableEditor.Models.FileSettings;
using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using RedmineTableEditor.Models;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class AssignedToColorsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<bool> IsEnabledClosed { get; set; }
        public EditableGridViewModel<AssignedToColorViewModel, AssignedToColorModel> Items { get; }

        public ReactiveCommand RowDropCommand { get; set; }

        public ReactiveProperty<bool> IsEdited { get; set; }

        public AssignedToColorsModel Model { get; }

        /// <summary>
        /// AssignedToColorViewModel に引数なしのコンストラクタを定義するため static で保持する
        /// </summary>
        public static RedmineManager Redmine { get; set; }

        public ReadOnlyReactivePropertySlim<ObservableCollection<IdentifiableName>> Users { get; set; }

        [Obsolete("Design Only", true)]
        public AssignedToColorsViewModel(){}

        public AssignedToColorsViewModel(AssignedToColorsModel model, RedmineManager redmine)
        {
            Redmine = redmine;
            this.Model = model;
            IsEnabled = model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables);
            IsEnabledClosed = model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabledClosed).AddTo(disposables);
            Items = new EditableGridViewModel<AssignedToColorViewModel, AssignedToColorModel>(model.Items, a => new AssignedToColorViewModel(a), a => a.Model);
            Items.CollectionChangedAsObservable().SubscribeWithErr(_ => RowDropCommand.Execute()).AddTo(disposables);

            RowDropCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                Items.Indexed().ToList().ForEach(a => a.v.Order.Value = a.i);
            }).AddTo(disposables);

            Users = redmine.ObserveProperty(a => a.Users).Where(u => u != null).Select(u => new ObservableCollection<IdentifiableName>(u)).ToReadOnlyReactivePropertySlim();

            IsEdited = new[]
            {
                IsEnabled.Skip(1).Select(_ => true),
                Items.CollectionChangedAsObservable().Select(_ => true),
                Items.ObserveElementObservableProperty(a => a.IsEdited).Where(a => a.Value).Select(_ => true),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);

            IsEdited.Where(a => !a).SubscribeWithErr(_ =>
            {
                Items.ToList().ForEach(a => a.IsEdited.Value = false);
            });
        }
    }
}
