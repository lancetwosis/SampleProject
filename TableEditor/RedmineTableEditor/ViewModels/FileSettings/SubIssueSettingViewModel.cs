using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Enums;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Enums;
using System.Collections.ObjectModel;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Standard;
using ObservableCollectionSync;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class SubIssueSettingViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<int> Order { get; set; }
        public ReactivePropertySlim<int> TrackerId { get; set; }
        public ReactivePropertySlim<string> Title { get; set; }
        public ReactivePropertySlim<string> Subject { get; set; }
        public ReactivePropertySlim<StringCompareType> SubjectCompare { get; set; }
        public ObservableCollectionSync<IssueStatus, int> SelectedStatuses { get; set; }
        public ReactivePropertySlim<StatusCompareType> StatusCompare { get; set; }

        public ReactiveProperty<bool> IsEdited { get; set; }
        public SubIssueSettingModel Model { get; internal set; }

        public SubIssueSettingViewModel()
        {
            Model = new SubIssueSettingModel();
            setBinding();
        }

        public SubIssueSettingViewModel(SubIssueSettingModel model)
        {
            this.Model = model;
            setBinding();
        }

        private void setBinding()
        {
            IsEnabled = Model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables);
            Order = Model.ToReactivePropertySlimAsSynchronized(a => a.Order).AddTo(disposables);
            TrackerId = Model.ToReactivePropertySlimAsSynchronized(a => a.TrackerId).AddTo(disposables);
            Title = Model.ToReactivePropertySlimAsSynchronized(a => a.Title).AddTo(disposables);
            Subject = Model.ToReactivePropertySlimAsSynchronized(a => a.Subject).AddTo(disposables);
            SubjectCompare = Model.ToReactivePropertySlimAsSynchronized(a => a.SubjectCompare).AddTo(disposables);
            SelectedStatuses = new ObservableCollectionSync<IssueStatus, int>(Model.StatusIds, i => SubIssueSettingsViewModel.Redmine.Cache.Statuss.SingleOrDefault(b => b.Id == i), i => i.Id).AddTo(disposables);
            StatusCompare = Model.ToReactivePropertySlimAsSynchronized(a => a.StatusCompare).AddTo(disposables);

            IsEdited = new[]
            {
                IsEnabled.Skip(1).Select(_ => true),
                Order.Skip(1).Select(_ => true),
                TrackerId.Skip(1).Select(_ => true),
                Title.Skip(1).Select(_ => true),
                Subject.Skip(1).Select(_ => true),
                SubjectCompare.Skip(1).Select(_ => true),
                SelectedStatuses.CollectionChangedAsObservable().Select(_ => true),
                StatusCompare.Skip(1).Select(_ => true),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);
        }
    }
}

