using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class CreateTicketSettingsViewModel : Bases.SettingsViewModelBase<CreateTicketSettingsModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReadOnlyReactivePropertySlim<CustomFieldSettingViewModel> DetectionProcess { get; set; }

        public ReactivePropertySlim<bool> NeedsOutlookIntegration { get; set; }
        public ReactivePropertySlim<bool> NeedsGitIntegration { get; set; }

        public ReactivePropertySlim<MyTracker> OpenTracker { get; set; }
        public ReactivePropertySlim<IdName> OpenStatus { get; set; }
        public ReactivePropertySlim<IdName> DefaultStatus { get; set; }
        public ReadOnlyReactivePropertySlim<CustomFieldSettingViewModel<ReviewMethodCustomField, ReviewMethodValue>> ReviewMethod { get; set; }

        public ReactivePropertySlim<MyTracker> RequestTracker { get; set; }
        public ReadOnlyReactivePropertySlim<CustomFieldSettingViewModel> IsRequired { get; set; }

        public ReactivePropertySlim<MyTracker> PointTracker { get; set; }
        public ReadOnlyReactivePropertySlim<CustomFieldSettingViewModel> SaveReviewer { get; set; }

        public ReadOnlyReactivePropertySlim<List<MyTracker>> Trackers { get; set; }
        public ReadOnlyReactivePropertySlim<List<IdName>> Statuses { get; set; }

        public CreateTicketSettingsViewModel(CreateTicketSettingsModel model) : base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            DetectionProcess = model.ToReadOnlyViewModel(a => a.DetectionProcess, a => new CustomFieldSettingViewModel(a)).AddTo(disposables);
            NeedsOutlookIntegration = model.ToReactivePropertySlimAsSynchronized(m => m.NeedsOutlookIntegration).AddTo(disposables);
            NeedsGitIntegration = model.ToReactivePropertySlimAsSynchronized(m => m.NeedsGitIntegration).AddTo(disposables);

            OpenTracker = model.ToReactivePropertySlimAsSynchronized(m => m.OpenTracker).AddTo(disposables);
            OpenStatus = model.ToReactivePropertySlimAsSynchronized(m => m.OpenStatus).AddTo(disposables);
            DefaultStatus = model.ToReactivePropertySlimAsSynchronized(m => m.DefaultStatus).AddTo(disposables);
            ReviewMethod = model.ToReadOnlyViewModel(a => a.ReviewMethod, 
                a => new CustomFieldSettingViewModel<ReviewMethodCustomField, ReviewMethodValue>(a)).AddTo(disposables);

            RequestTracker = model.ToReactivePropertySlimAsSynchronized(m => m.RequestTracker).AddTo(disposables);
            IsRequired = model.ToReadOnlyViewModel(a => a.IsRequired, a => new CustomFieldSettingViewModel(a)).AddTo(disposables);

            PointTracker = model.ToReactivePropertySlimAsSynchronized(m => m.PointTracker).AddTo(disposables);
            SaveReviewer = model.ToReadOnlyViewModel(a => a.SaveReviewer, a => new CustomFieldSettingViewModel(a)).AddTo(disposables);

            // 選択項目
            Trackers = CacheTempManager.Default.MyTrackers.Where(a => a != null)
                .Select(a => new List<MyTracker>(new[] { MyTracker.USE_PARENT_TRACKER }).Concat(a).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Trackers.Where(a => a != null).Subscribe(tracker => {
                OpenTracker.Value = tracker.FirstOrDefault(OpenTracker.Value);
                RequestTracker.Value = tracker.FirstOrDefault(RequestTracker.Value);
                PointTracker.Value = tracker.FirstOrDefault(PointTracker.Value);
            } ).AddTo(disposables);
            Statuses = CacheTempManager.Default.Statuss.Where(a => a != null)
                .Select(a => a.Select(b => new IdName(b)).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Statuses.Where(a => a != null).Subscribe(statuses =>
            {
                OpenStatus.Value = statuses.FirstOrDefault(OpenStatus.Value);
                DefaultStatus.Value = statuses.FirstOrDefault(DefaultStatus.Value);
            }).AddTo(disposables);

            CacheTempManager.Default.CustomFields.Where(a => a != null).SubscribeWithErr(cfs => {
                var boolCustomFields = cfs.Where(c => c.IsIssueType() && c.IsBoolFormat()).Select(c => new MyCustomField(c)).ToList();
                var listCustomFields = cfs.Where(c => c.IsIssueType() && c.IsListFormat()).Select(c => new MyCustomField(c)).ToList();
                var userCustomFields = cfs.Where(c => c.IsIssueType() && c.IsUserFormat()).Select(c => new MyCustomField(c)).ToList();
                model.Update(boolCustomFields, listCustomFields, userCustomFields);
            }).AddTo(disposables);
        }
    }
}
