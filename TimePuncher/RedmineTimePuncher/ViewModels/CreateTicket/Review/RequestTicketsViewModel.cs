using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class RequestTicketsViewModel
        : RequestTicketsViewModelBase<AssigneesViewModel, PeriodViewModel, RequestTicketsModel, TargetTicketModel, PeriodModel>
    {
        public ReactivePropertySlim<AssigneeViewModel> Organizer { get; set; }

        public SelfReviewViewModel SelfReview { get; set; }

        public PreviewViewModel<RequestTicketsModel, PeriodModel> ReviewTarget { get; set; }
        public ReadOnlyReactivePropertySlim<bool> NeedsGitIntegration { get; set; }
        public ReactivePropertySlim<string> MergeRequestUrl { get; set; }

        public CustomFieldsViewModel CustomFields { get; set; }

        public RequestTicketsViewModel(RequestTicketsModel requests, TargetTicketModel target) : base(requests)
        {
            Assignee = new AssigneesViewModel(requests.Assignees, target).AddTo(disposables);

            Organizer = requests.ToReactivePropertySlimAsSynchronized(m => m.Organizer,
                m => m != null ? new AssigneeViewModel(m) : null,
                vm => vm != null ? vm.Model : requests.Organizer).AddTo(disposables);
            Assignee.AllAssignees.Where(a => a != null).Subscribe(all =>
            {
                if (target.Ticket == null) return;

                Organizer.Value = requests.Organizer != null ?
                    all.FirstOrDefault(m => m.Model.Id == requests.Organizer.Id, null) :
                    all.FirstOrDefault(m => m.Model.Id == target.Ticket.AssignedTo.Id, null);
            }).AddTo(disposables);

            SelfReview = new SelfReviewViewModel(requests.SelfReview).AddTo(disposables);
            Period = new PeriodViewModel(requests.Period).AddTo(disposables);

            target.ObserveProperty(m => m.Ticket).CombineLatest(
                SettingsModel.Default.ObserveProperty(m => m.CreateTicket.DetectionProcess.IsEnabled),
                target.ObserveProperty(a => a.Process), (t, isEnabled, v) => (t, isEnabled, v))
                .Where(a => a.t != null).SubscribeWithErr(p =>
                {
                    var processName = (p.isEnabled && p.v != null) ? p.v.Label : "";
                    Title.Value = $"{processName}{Resources.Review} : {p.t.Subject}";
                }).AddTo(disposables);

            ReviewTarget = new PreviewViewModel<RequestTicketsModel, PeriodModel>(requests, m => m.ReviewTarget).AddTo(disposables);
            NeedsGitIntegration = SettingsModel.Default.ObserveProperty(a => a.CreateTicket.NeedsGitIntegration).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            MergeRequestUrl = requests.ToReactivePropertySlimAsSynchronized(m => m.MergeRequestUrl).AddTo(disposables);
            MergeRequestUrl.Pairwise().CombineLatest(NeedsGitIntegration, (url, needsIntegrate) => (url, needsIntegrate)).SubscribeWithErr(p =>
            {
                if (!p.needsIntegrate || ReviewTarget == null || string.IsNullOrEmpty(p.url.NewItem))
                    return;

                var oldTarget = ReviewTarget.InputText.Value;
                if (!string.IsNullOrEmpty(p.url.OldItem) && oldTarget.Contains(p.url.OldItem))
                {
                    ReviewTarget.InputText.Value = oldTarget.Replace(p.url.OldItem, p.url.NewItem);
                }
                else
                {
                    var label = p.url.NewItem.Contains("gitlab") ? "Merge Request URL" : "Pull Request URL";
                    var link = CacheManager.Default.MarkupLang.CreateLink(label, p.url.NewItem);
                    ReviewTarget.InputText.Value = oldTarget + $"{Environment.NewLine}{Environment.NewLine}{link}";
                }
            });

            CustomFields = new CustomFieldsViewModel(requests.CustomFields, target).AddTo(disposables);

            target.ObserveProperty(m => m.Ticket).Skip(1).Where(t => t != null).SubscribeWithErr(t =>
            {
                ReviewTarget.InputText.Value = CacheManager.Default.MarkupLang.CreateTicketLink(t);
                MergeRequestUrl.Value = "";
            }).AddTo(disposables);
        }

        public override void Clear()
        {
            base.Clear();

            Organizer.Value = null;
            CustomFields.Clear();
            ReviewTarget.Clear();
            MergeRequestUrl.Value = "";
        }

        public void ApplyTemplate(RequestTicketsModel template)
        {
            Assignee.ApplyTemplate(template.Assignees);
            Organizer.Value = template.Organizer != null ? new AssigneeViewModel(template.Organizer).AddTo(disposables) : null;
            SelfReview.ApplyTemplate(template.SelfReview);
            Period.ApplyTemplate(template.Period);
            CustomFields.ApplyTemplate(template.CustomFields);
            Description.InputText.Value = template.Description;
        }
    }
}
