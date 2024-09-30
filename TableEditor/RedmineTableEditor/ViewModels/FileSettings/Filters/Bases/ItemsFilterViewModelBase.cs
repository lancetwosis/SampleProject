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
    public class ItemsFilterViewModelBase<T> : FilterViewModelBase<T>
        where T : ItemsFilterModelBase, new()
    {
        public ReactiveCommand ChangeModeCommand { get; set; }

        public bool NowEditing { get; set; }
        public ReactiveCommand EditCommand { get; set; }
        public ReactiveCommand SaveCommand { get; set; }
        public TwinListBoxViewModel<FilterItemModel> ItemsTwinListBox { get; set; }

        public ItemsFilterViewModelBase(T model) : base(model)
        {
            ChangeModeCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                Model.IsMultiple = !Model.IsMultiple;
            }).AddTo(disposables);

            EditCommand = new ReactiveCommand().WithSubscribe(() => NowEditing = true).AddTo(disposables);
            SaveCommand = new ReactiveCommand().WithSubscribe(() => NowEditing = false).AddTo(disposables);

            new[]
            {
                Model.ObserveProperty(m => m.IsSelected).Skip(1),
                Model.ObserveProperty(m => m.IsChecked).Skip(1),
                Model.ObserveProperty(m => m.CompareType).Skip(1).Select(_ => true),
                // プロジェクトの設定の変更により未選択状態で更新されても編集されたとはカウントしない
                Model.ObserveProperty(m => m.SelectedItem).Skip(1).Select(_ => Model.IsSelected && !Model.IsMultiple).Where(a => a),
                Model.Items.CollectionChangedAsObservable().Select(_ => Model.IsSelected && Model.IsMultiple).Where(a => a),
                Model.ObserveProperty(m => m.IsMultiple).Skip(1)
            }.Merge().SubscribeWithErr(_ => IsEdited.Value = true).AddTo(disposables);

            ErrorMessage = Model.ObserveProperty(m => m.IsSelected).CombineLatest(
                    Model.ObserveProperty(m => m.IsChecked),
                    Model.ObserveProperty(m => m.CompareType),
                    Model.ObserveProperty(m => m.SelectedItem),
                    Model.Items.CollectionChangedAsObservable().StartWithDefault(),
                    Model.ObserveProperty(m => m.IsMultiple),
                     (_1, _2, _3, _4, _5, _6) => Model.Validate())
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        CompositeDisposable myDisposables2 = null;
        protected void updateAllItems(List<FilterItemModel> allItems)
        {
            myDisposables2?.Dispose();
            myDisposables2 = new CompositeDisposable().AddTo(myDisposables);

            Model.AllItems = allItems;
            if (Model.AllItems.Count > 0 &&
                (Model.SelectedItem == null || !Model.IsSelected))
            {
                Model.SelectedItem = Model.AllItems.First();
            }

            ItemsTwinListBox = new TwinListBoxViewModel<FilterItemModel>(Model.AllItems, Model.Items).AddTo(myDisposables2);
        }
    }
}
