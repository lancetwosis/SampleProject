using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings.Filters;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings.Filters.Bases
{
    public class NumericFilterViewModelBase<T, TValue> : FilterViewModelBase<T>
        where T : NumericFilterModelBase<TValue>, new()
        where TValue : IComparable<TValue>
    {
        public ReactivePropertySlim<string> Value { get; set; }
        public ReactivePropertySlim<string> Lower { get; set; }
        public ReactivePropertySlim<string> Upper { get; set; }

        public NumericFilterViewModelBase(T model) : base(model)
        {
            Value = model.ToReactivePropertySlimAsSynchronized(m => m.Value).AddTo(disposables);
            Lower = model.ToReactivePropertySlimAsSynchronized(m => m.Lower).AddTo(disposables);
            Upper = model.ToReactivePropertySlimAsSynchronized(m => m.Upper).AddTo(disposables);

            new[]
            {
                Model.ObserveProperty(m => m.IsSelected).Skip(1),
                Model.ObserveProperty(m => m.IsChecked).Skip(1),
                Model.ObserveProperty(m => m.CompareType).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.Value).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.Lower).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.Upper).Skip(1).Select(_ => true),
            }.Merge().SubscribeWithErr(_ => IsEdited.Value = true).AddTo(disposables);

            ErrorMessage = Model.ObserveProperty(m => m.IsSelected).CombineLatest(
                    Model.ObserveProperty(m => m.IsChecked),
                    Model.ObserveProperty(m => m.CompareType),
                    Model.ObserveProperty(m => m.Value),
                    Model.ObserveProperty(m => m.Lower),
                    Model.ObserveProperty(m => m.Upper), (_1, _2, _3, _4, _5, _6) => Model.Validate())
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null)
        {
            // redmine や projects に伴う更新は不要なので何もしない
        }
    }
}
