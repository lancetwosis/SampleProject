using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings.Filters;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using RedmineTableEditor.Models.FileSettings.Filters.Custom;
using RedmineTableEditor.Models.FileSettings.Filters.Standard;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings.Filters.Custom
{
    public class CfBoolFilterViewModel : FilterViewModelBase<CfItemsFilterModel>
    {
        // 選択モードの切り替えボタンの表示非表示のフラグとして保持
        public ReactiveCommand ChangeModeCommand { get; set; } = null;

        public CfBoolFilterViewModel(CfItemsFilterModel model) : base(model)
        {
            new[]
{
                Model.ObserveProperty(m => m.IsSelected).Skip(1),
                Model.ObserveProperty(m => m.IsChecked).Skip(1),
                Model.ObserveProperty(m => m.CompareType).Skip(1).Select(_ => true),
                // プロジェクトの設定の変更により未選択状態で更新されても編集されたとはカウントしない
                Model.ObserveProperty(m => m.SelectedItem).Skip(1).Select(_ => Model.IsSelected && !Model.IsMultiple).Where(a => a),
            }.Merge().SubscribeWithErr(_ => IsEdited.Value = true).AddTo(disposables);

            ErrorMessage = Model.ObserveProperty(m => m.IsSelected).CombineLatest(
                    Model.ObserveProperty(m => m.IsChecked),
                    Model.ObserveProperty(m => m.CompareType),
                    Model.ObserveProperty(m => m.SelectedItem),
                     (_1, _2, _3, _4) => Model.Validate())
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Model.AllItems = new List<FilterItemModel>() { CfItemsFilterModel.YES, CfItemsFilterModel.NO };
            if (Model.SelectedItem == null || !Model.IsSelected)
            {
                Model.SelectedItem = Model.AllItems.First();
            }
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> _ = null)
        {
            // コンストラクタの実行のタイミングで Setup を行うので何もしない
        }
    }
}
