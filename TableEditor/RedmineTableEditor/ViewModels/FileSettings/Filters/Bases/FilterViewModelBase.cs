using LibRedminePower.Enums;
using LibRedminePower.Extentions;
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
    public interface IFilterViewModel
    {
        ReactivePropertySlim<bool> IsEnabled { get; set; }
        ReactivePropertySlim<bool> IsSelected { get; set; }
        ReactivePropertySlim<bool> IsEdited { get; set; }
        ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        ReadOnlyReactivePropertySlim<bool> NeedsFilter { get; set; }

        void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null);
    }

    public class FilterViewModelBase<T> : LibRedminePower.ViewModels.Bases.ViewModelBase, IFilterViewModel
        where T : FilterModelBase, new()
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<bool> IsSelected { get; set; }
        public ReactivePropertySlim<bool> IsEdited { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<bool> NeedsFilter { get; set; }

        public ReadOnlyReactivePropertySlim<bool> NeedsInput { get; set; }

        public T Model { get; set; }

        public FilterViewModelBase(T model)
        {
            Model = model;

            // CompareType が不正な値だったらデフォルトに戻す（IssueCompareType の定義順をいじると発生）
            if (!Model.CompareTypes.Contains(Model.CompareType))
                Model.SetDefaultCompareType();

            NeedsInput = Model.ObserveProperty(m => m.CompareType).Select(t => t.NeedsInput()).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsEnabled = Model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);

            IsSelected = Model.ToReactivePropertySlimAsSynchronized(m => m.IsSelected).AddTo(disposables);
            IsSelected.SubscribeWithErr(isSelected =>
            {
                // 未選択なら初期状態に戻す
                if (!isSelected)
                    Model.SetDefaults();
            });

            NeedsFilter = Model.ObserveProperty(m => m.NeedsFilter).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsEdited = new ReactivePropertySlim<bool>().AddTo(disposables);
        }

        protected CompositeDisposable myDisposables;
        public virtual void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);
        }

        public override string ToString()
        {
            return Model.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is FilterViewModelBase<T> other &&
                   Model?.ToJson() == other.Model?.ToJson();
        }

        public override int GetHashCode()
        {
            return -623947254 + EqualityComparer<T>.Default.GetHashCode(Model);
        }
    }
}
