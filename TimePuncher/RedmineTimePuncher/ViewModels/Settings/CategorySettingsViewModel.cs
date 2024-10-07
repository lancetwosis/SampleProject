using AutoMapper;
using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using LibRedminePower.Logging;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class CategorySettingsViewModel : Bases.SettingsViewModelBase<CategorySettingsModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReadOnlyReactivePropertySlim<List<MyTracker>> MyTrackers { get; set; }
        public ReadOnlyReactivePropertySlim<List<Project>> Projects { get; set; }
        public ReadOnlyReactivePropertySlim<List<Tracker>> Trackers { get; set; }
        public ReadOnlyReactivePropertySlim<List<IssueStatus>> Statuss { get; set; }
        public ReadOnlyReactivePropertySlim<EditableGridViewModel<CategorySettingViewModel>> Items { get; set; }
        public ReactivePropertySlim<bool> IsAutoSameName { get; set; }

        public CategorySettingsViewModel(CategorySettingsModel model) :base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            MyTrackers = CacheTempManager.Default.MyTrackers.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Projects = CacheTempManager.Default.Projects.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Trackers = CacheTempManager.Default.Trackers.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Statuss = CacheTempManager.Default.Statuss.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Items =
                // ModelのItemsが更新されたら（インポートなどでも）
                model.ObserveProperty(a => a.Items).CombineLatest(
                    // かつ、Cacheが更新されたら
                    CacheTempManager.Default.Updated.Where(a => a != DateTime.MinValue), 
                    (items, _) => {
                        // まずは、Redmineの作業分類に基づいて、ModelのItemsを追加／削除する。
                        model.UpdateItems(CacheTempManager.Default.TimeEntryActivities.Value);
                        // 次に、更新したModelのItemsから、編集可能グリッドを生成する。
                        return new EditableGridViewModel<CategorySettingViewModel>(
                            items.OrderBy(b => b.Order).Select(
                                // グリッド生成時の各ViewModelは親のDisposeに合わせる
                                a => new CategorySettingViewModel(a).AddTo(this.disposables)).ToList());
                    }).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsAutoSameName = model.ToReactivePropertySlimAsSynchronized(a => a.IsAutoSameName).AddTo(disposables);
        }

        /// <summary>
        /// 設定の表示順を CategorySettingsModel の Order に適用する
        /// </summary>
        public void ApplyOrders()
        {
            Items.Value?.Indexed().ToList().ForEach(a => a.v.SetOrder(a.i));
        }
    }
}