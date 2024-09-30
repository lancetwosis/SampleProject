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
    public class DateFilterViewModelBase<T> : FilterViewModelBase<T>
        where T : DateFilterModelBase, new()
    {
        public ReactivePropertySlim<DateTime> Date { get; set; }
        public ReactivePropertySlim<DateTime> From { get; set; }
        public ReactivePropertySlim<DateTime> To { get; set; }
        public ReactivePropertySlim<string> LastNDays { get; set; }
        public ReactivePropertySlim<string> NextNDays { get; set; }

        public DateFilterViewModelBase(T model) : base(model)
        {
            Date = model.ToReactivePropertySlimAsSynchronized(m => m.Date).AddTo(disposables);
            From = model.ToReactivePropertySlimAsSynchronized(m => m.From).AddTo(disposables);
            To = model.ToReactivePropertySlimAsSynchronized(m => m.To).AddTo(disposables);
            LastNDays = model.ToReactivePropertySlimAsSynchronized(m => m.LastNDays).AddTo(disposables);
            NextNDays = model.ToReactivePropertySlimAsSynchronized(m => m.NextNDays).AddTo(disposables);

            new[]
            {
                Model.ObserveProperty(m => m.IsSelected).Skip(1),
                Model.ObserveProperty(m => m.IsChecked).Skip(1),
                Model.ObserveProperty(m => m.CompareType).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.Date).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.From).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.To).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.LastNDays).Skip(1).Select(_ => true),
                Model.ObserveProperty(m => m.NextNDays).Skip(1).Select(_ => true),
            }.Merge().SubscribeWithErr(_ => IsEdited.Value = true).AddTo(disposables);

            ErrorMessage = Model.ObserveProperty(m => m.IsSelected).CombineLatest(
                    Model.ObserveProperty(m => m.IsChecked),
                    Model.ObserveProperty(m => m.CompareType),
                    Model.ObserveProperty(m => m.From),
                    Model.ObserveProperty(m => m.To),
                    Model.ObserveProperty(m => m.LastNDays),
                    Model.ObserveProperty(m => m.NextNDays), (_1, _2, _3, _4, _5, _6, _7) => Model.Validate())
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null)
        {
            // redmine や projects に伴う更新は不要なので何もしない
        }
    }
}
