using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class SubIssueSettingsViewModel : IssueSettingsViewModelBase
    {
        public EditableGridViewModel<SubIssueSettingViewModel, SubIssueSettingModel> Items { get; set; }

        public ReactiveCommand RowDropCommand { get; set; }
        public ReactiveProperty<bool> IsEdited { get; set; }

        /// <summary>
        /// SubIssueSettingViewModel に引数なしのコンストラクタを定義するため static で保持する
        /// </summary>
        public static RedmineManager Redmine { get; set; }
        public ReadOnlyReactivePropertySlim<ObservableCollection<Tracker>> Trackers { get; set; }
        public ReadOnlyReactivePropertySlim<ObservableCollection<IssueStatus>> Statuses { get; set; }

        [Obsolete("Design Only", true)]
        public SubIssueSettingsViewModel() {}

        public SubIssueSettingsViewModel(SubIssueSettingsModel model, RedmineManager redmine) : base(model, redmine)
        {
            Redmine = redmine;

            Items = new EditableGridViewModel<SubIssueSettingViewModel, SubIssueSettingModel>(model.Items, a => new SubIssueSettingViewModel(a), a => a.Model).AddTo(disposables);
            Items.CollectionChangedAsObservable().Subscribe(_ => RowDropCommand.Execute()).AddTo(disposables);

            RowDropCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                Items.Indexed().ToList().ForEach(a => a.v.Order.Value = a.i);
            }).AddTo(disposables);

            Trackers = redmine.ObserveProperty(a => a.Trackers).Where(a => a != null).Select(a =>
            {
                var ts = new ObservableCollection<Tracker>(a);
                ts.Insert(0, Models.TicketFields.Standard.Tracker.NOT_SPECIFIED);
                return ts;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Statuses = redmine.ObserveProperty(a => a.Statuses).Where(a => a != null)
                .Select(a => new ObservableCollection<IssueStatus>(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // 変更を刈り取る。
            IsEdited = new[]
            {
                Items.CollectionChangedAsObservable().Select(_ => true),
                Items.ObserveElementObservableProperty(a => a.IsEdited).Where(a => a.Value).Select(_ => true),
                this.ObserveProperty(a => a.VisibleProps.IsEdited.Value).Where(a => a),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);

            IsEdited.Where(a => !a).Subscribe(_ =>
            {
                Items.ToList().ForEach(a => a.IsEdited.Value = false);

                if (VisibleProps != null)
                    VisibleProps.IsEdited.Value = false;
            }).AddTo(disposables);
        }
    }
}
