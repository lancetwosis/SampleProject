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
    public class TextFilterViewModelBase<T> : FilterViewModelBase<T>
        where T : TextFilterModelBase, new()
    {
        public ReactivePropertySlim<string> Text { get; set; }

        public TextFilterViewModelBase(T model) : base(model)
        {
            Text = model.ToReactivePropertySlimAsSynchronized(m => m.Text).AddTo(disposables);

            new[]
            {
                Model.ObserveProperty(m => m.IsSelected).Skip(1),
                Model.ObserveProperty(m => m.IsChecked).Skip(1),
                Model.ObserveProperty(m => m.CompareType).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.Text).Skip(1).Select(_ => true),
            }.Merge().SubscribeWithErr(_ => IsEdited.Value = true).AddTo(disposables);

            ErrorMessage = Model.ObserveProperty(m => m.IsSelected).CombineLatest(
                    Model.ObserveProperty(m => m.IsChecked),
                    Model.ObserveProperty(m => m.CompareType),
                    Model.ObserveProperty(m => m.Text), (_1, _2, _3, _4) => Model.Validate())
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null)
        {
            // redmine や projects に伴う更新は不要なので何もしない
        }
    }
}
