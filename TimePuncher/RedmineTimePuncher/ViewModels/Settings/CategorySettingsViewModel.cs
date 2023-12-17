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
        public EditableGridViewModel<CategorySettingViewModel> Items { get; set; }

        public ReactivePropertySlim<bool> IsAutoSameName { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public CategorySettingsViewModel(CategorySettingsModel model, ReactivePropertySlim<string> errorMessage) :base(model)
        {
            var error1 = errorMessage.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var error2 = new ReactivePropertySlim<string>().AddTo(disposables);
            ErrorMessage = new IObservable<string>[] { error1, error2 }.CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            var trackers = CacheManager.Default.Trackers.Value;
            CategorySettingViewModel.Trackers = trackers.Select(t => new MyTracker(t)).ToList();
            AssignRuleViewModel.Projects = CacheManager.Default.Projects.Value;
            AssignRuleViewModel.Trackers = trackers;
            AssignRuleViewModel.Statuss = CacheManager.Default.Statuss.Value;

            setUpItemsSource(model, CacheManager.Default.TimeEntryActivities.Value);

            IsAutoSameName = model.ToReactivePropertySlimAsSynchronized(a => a.IsAutoSameName).AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(() =>
                setUpItemsSource(model, CacheManager.Default.TimeEntryActivities.Value)).AddTo(disposables);
        }

        private void setUpItemsSource(CategorySettingsModel model, List<TimeEntryActivity> timeEntries)
        {
            // Redmineの情報とModelを同期する。
            model.UpdateItems(timeEntries);
            Items = new EditableGridViewModel<CategorySettingViewModel>(model.Items.OrderBy(a => a.Order).Select(a => new CategorySettingViewModel(a)).ToList());
        }
    }
}