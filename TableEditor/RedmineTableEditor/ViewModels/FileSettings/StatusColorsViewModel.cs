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
using RedmineTableEditor.Models;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class StatusColorsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ObservableCollection<StatusColorViewModel> Items { get; private set; }

        public ReactiveProperty<bool> IsEdited { get; set; }

        public StatusColorsModel Model { get; }

        [Obsolete("Design Only", true)]
        public StatusColorsViewModel(){}

        public StatusColorsViewModel(StatusColorsModel model, RedmineManager redmine)
        {
            Model = model;

            IsEnabled = model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables);

            Items = new ObservableCollection<StatusColorViewModel>(
                redmine.Cache.Statuss.Select(s =>
                {
                    var colorModel = model.Items.SingleOrDefault(a => a.Id == s.Id);
                    if (colorModel == null)
                    {
                        colorModel = new StatusColorModel() { Id = s.Id, Color = System.Drawing.Color.Transparent };
                        model.Items.Add(colorModel);
                    }
                    return new StatusColorViewModel(s, colorModel);
                }));

            IsEdited = new[]
            {
                IsEnabled.Skip(1).Select(_ => true),
                Items.CollectionChangedAsObservable().Select(_ => true),
                Items.ObserveElementPropertyChanged().Select(_ => true),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);
        }
    }
}
