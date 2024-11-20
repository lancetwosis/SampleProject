using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using ObservableCollectionSync;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Enums;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class ReviewViewModel : RequestViewModelBase<TargetTicketViewModel, AssigneesViewModel, PeriodViewModel, RequestTicketsViewModel,
                                                        TargetTicketModel, RequestTicketsModel, PeriodModel>
    {
        public ReactivePropertySlim<ReviewStatus> Status { get; set; }
        public ReadOnlyReactivePropertySlim<bool> NowSelfReviewing { get; set; }

        public ReactivePropertySlim<string> Name { get; set; }

        public ReactiveCommandSlim ContinueReviewCommand { get; set; }
        public ReactiveCommandSlim CancelReviewCommand { get; set; }

        public ReviewModel Model { get; set; }

        public ReviewViewModel(ReviewModel model)
            : base(new TargetTicketViewModel(model.Target), new RequestTicketsViewModel(model.Requests, model.Target))
        {
            Model = model;

            Status = model.ToReactivePropertySlimAsSynchronized(m => m.Status).AddTo(disposables);
            NowSelfReviewing = Status.Select(s => s == ReviewStatus.SelfReviewIng).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Name = model.ToReactivePropertySlimAsSynchronized(m => m.Name).AddTo(disposables);
            Target.Ticket.SubscribeWithErr(t =>
            {
                Name.Value = t != null ? $"#{t.Id}" : Resources.ReviewNewReview;
            }).AddTo(disposables);

            CanCreate = new[] {
                IsBusy.Select(i => i ? "" : null),
                Target.IsValid,
                Target.Process.IsValid,
                NowSelfReviewing.Select(s => s ? "" : null),
                Requests.Period.IsValid,
                Requests.Assignee.IsValid,
                Requests.CustomFields.IsValid,
            }.CombineLatestFirstOrDefault(a => a != null).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            
            ContinueReviewCommand = Status.Select(s => s == ReviewStatus.SelfReviewIng).ToReactiveCommandSlim().WithSubscribe(async () =>
            {
                var m = Requests.SelfReview.Model;
                try
                {
                    var selfTicket = await Task.Run(() => RedmineManager.Default.Value.GetTicketsById(m.SelfTicket.Id.ToString()));
                    var status = CacheManager.Default.Statuss.First(s => s.Id == selfTicket.Status.Id);
                    if (!status.IsClosed)
                    {
                        var r = MessageBoxHelper.ConfirmWarning(Resources.ReviewErrMsgSelfNotFinished, MessageBoxHelper.ButtonType.OkCancel);
                        if (!r.HasValue || !r.Value)
                            return;
                    }
                }
                catch (System.Exception e)
                {
                    Status.Value = ReviewStatus.Preparing;
                    throw new ApplicationException(Resources.ReviewErrMsgSelfNotFound, e);
                }

                Status.Value = ReviewStatus.Preparing;
                using (IsBusy.ProcessStart(Resources.ProgressMsgCreatingIssues))
                {
                    await createRequestTicketsAsync(m.OpenTicket, m.RequestTracker, m.PointTracker, m.Desctription, m.ShowAllUrl);
                }
            }).AddTo(disposables);

            CancelReviewCommand = Status.Select(s => s == ReviewStatus.SelfReviewIng).ToReactiveCommandSlim().WithSubscribe(() =>
            {
                var msg = string.Format(Resources.ReviewMsgSelftConfirmCancel, Requests.SelfReview.TicketId.Value);
                var r = MessageBoxHelper.ConfirmInformation(msg, MessageBoxHelper.ButtonType.OkCancel);
                if (!r.HasValue || !r.Value)
                    return;

                Status.Value = ReviewStatus.Preparing;
            }).AddTo(disposables);
        }

        public override void Clear()
        {
            base.Clear();
            Status.Value = ReviewStatus.Preparing;
        }

        public void AdjustSchedule()
        {
            var exception = createOutlookAppointment(
                $"({Resources.ReviewForShcheduleAdjust}) {Requests.Title}",
                string.Format(Resources.ReviewMsgForAdjustSchedule, ApplicationInfo.Title),
                i =>
                {
                    i.CloseEvent += (ref bool cancel) =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Requests.Period.StartDateTime.Value = i.Start;
                            Requests.Period.DueDateTime.Value = i.End;
                        });
                    };
                });
            if (exception != null)
                throw exception;
        }

        public async Task CreateTicketAsync()
        {
            using (IsBusy.ProcessStart(Resources.ProgressMsgCreatingIssues))
            {
                // 説明の転記機能のチェック
                var transPrgs = await transcribeAsync(Target.GetProcess(),
                                                      SettingsModel.Default.TranscribeSettings.OpenTranscribe,
                                                      SettingsModel.Default.TranscribeSettings.RequestTranscribe);
                if (transPrgs == null)
                    return;
                var openTransPrg = transPrgs.IsNotEmpty() ? transPrgs[0] : ""; ;
                var requestTransPrg = transPrgs.IsNotEmpty() ? transPrgs[1] : "";

                // 設定のトラッカーが選択中のチケットのプロジェクトで有効かどうかのチェック
                var idNames = convertTrackers((Resources.SettingsReviOpenTicket, SettingsModel.Default.CreateTicket.OpenTracker),
                                              (Resources.SettingsReviRequestTicket, SettingsModel.Default.CreateTicket.RequestTracker),
                                              (Resources.SettingsReviPointTicket, SettingsModel.Default.CreateTicket.PointTracker));
                if (idNames == null)
                    return;
                var openTracker = idNames[0];
                var requestTracker = idNames[1];
                var pointTracker = idNames[2];

                // 開催チケットの作成
                var p = Target.Ticket.Value.CreateChildTicket();
                p.Author = CacheManager.Default.MyUser.ToIdentifiableName();
                p.AssignedTo = Requests.Organizer.Value.Model.ToIdentifiableName();
                p.Subject = Requests.Title.Value;
                p.Tracker = openTracker;
                p.StartDate = Requests.Period.GetStart().GetDateOnly();
                p.DueDate = Requests.Period.GetDue().GetDateOnly();
                p.Description = "";
                p.Status = SettingsModel.Default.CreateTicket.OpenStatus.ToIdentifiableName();
                p.CustomFields = createCustomFields(Requests.CustomFields.Open.GetIssueCustomFields());

                var openTicket = new MyIssue(await Task.Run(() => RedmineManager.Default.Value.CreateTicket(p)));

                // 開催チケットの説明を更新
                var detectProcPrg = Target.Process.CreatePrgForTicket();
                var reviewMethodPrg = Requests.Period.CreatePrgForTicket();
                var targetPrg = CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewDelivarables, Requests.ReviewTarget.Value.SplitLines());
                var createPointPrg = createPointParagraph(openTicket, pointTracker, null);
                var showAllUrl = SettingsModel.Default.ReviewIssueList.CreateShowAllPointIssuesUrl(openTicket.RawIssue, pointTracker.Id);
                var showAllPointsPrg = CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewPointsList, CacheManager.Default.MarkupLang.CreateLink(Resources.ReviewMsgReferPointsAtHere, showAllUrl));
                var description = joinIfNotNullOrWhiteSpace(detectProcPrg, reviewMethodPrg, targetPrg, createPointPrg, showAllPointsPrg, openTransPrg);
                if (Requests.NeedsGitIntegration.Value && !string.IsNullOrEmpty(Requests.MergeRequestUrl.Value))
                {
                    description += $"{Environment.NewLine}{Environment.NewLine}";
                    description += CacheManager.Default.MarkupLang.CreateCollapse("Do not edit the followings.", $"_merge_request_url={Requests.MergeRequestUrl.Value}");
                }
                openTicket.RawIssue.Description = description;
                await Task.Run(() => RedmineManager.Default.Value.UpdateTicket(openTicket.RawIssue));

                // 依頼チケットの作成
                description = joinIfNotNullOrWhiteSpace(
                    string.IsNullOrEmpty(Requests.Description.Value) ? Resources.ReviewPleaseFollwings : Requests.Description.Value,
                    detectProcPrg,
                    reviewMethodPrg,
                    targetPrg,
                    "{0}",
                    showAllPointsPrg,
                    requestTransPrg);

                // セルフレビューのチェック
                if (Requests.SelfReview.IsEnabled.Value)
                {
                    var organizer = Requests.Organizer.Value.Model.Clone();
                    organizer.IsRequired = true;
                    var selfTicket = await createRequestTicketAsync(organizer, openTicket, requestTracker, pointTracker, description, true);

                    Status.Value = ReviewStatus.SelfReviewIng;
                    Requests.SelfReview.Model.SelfTicket = new MyIssue(selfTicket);
                    Requests.SelfReview.Model.OpenTicket = openTicket;
                    Requests.SelfReview.Model.RequestTracker = requestTracker;
                    Requests.SelfReview.Model.PointTracker = pointTracker;
                    Requests.SelfReview.Model.Desctription = description;
                    Requests.SelfReview.Model.ShowAllUrl = showAllUrl;

                    Requests.SelfReview.Model.SelfTicket.GoToTicket();
                    return;
                }

                await createRequestTicketsAsync(openTicket, requestTracker, pointTracker, description, showAllUrl);
            }
        }

        private async Task createRequestTicketsAsync(MyIssue openTicket, IdentifiableName requestTracker, IdentifiableName pointTracker, string description, string showAllUrl)
        {
            // 各レビューアのチケット作成
            foreach (var r in Requests.Assignee.SelectedAssignees)
            {
                await createRequestTicketAsync(r.Model, openTicket, requestTracker, pointTracker, description, false);
            }

            // Outlook への予定の追加
            System.Exception failedToCreateAppointment = null;
            if (Requests.Period.NeedsOutlookIntegration.Value && Requests.Period.NeedsCreateOutlookAppointment.Value)
            {
                var createPointUrl = RedmineManager.Default.Value.CreatePointIssueURL(
                                        openTicket.RawIssue,
                                        pointTracker.Id,
                                        Target.Process.GetQueryString(),
                                        null,
                                        Requests.Period.GetQueryString(),
                                        Requests.CustomFields.Point.GetIssueCustomFieldQuries());
                var refKey = SettingsModel.Default.Appointment.Outlook.RefsKeywords.Split(",".ToCharArray()).FirstOrDefault();
                failedToCreateAppointment = createOutlookAppointment(
                    Requests.Title.Value,
                    joinIfNotNullOrWhiteSpace(
                        string.IsNullOrEmpty(Requests.Description.Value) ? Resources.ReviewPleaseFollwings : Requests.Description.Value,
                        Target.Process.CreatePrgForOutlook(),
                        Requests.Period.CreatePrgForOutlook(),
                        MarkupLangType.None.CreateParagraph(Resources.SettingsReviOpenTicket, openTicket.Url),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewTargetIssue, Target.Ticket.Value.Url),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewPointsAdd, createPointUrl),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewPointsList, showAllUrl),
                        Requests.Organizer.Value.Model.Name,
                        refKey != null ? $"{refKey} #{Target.Ticket.Value.Id}" : ""),
                    i => i.Save());
            }

            // レビュー対象チケットのステータスの更新
            var failedToUpdate = await applyStatusUnderRequestAsync();

            // 開催チケットを開く
            openTicket.GoToTicket();

            var errs = new[] { failedToCreateAppointment, failedToUpdate }.Where(a => a != null).ToList();
            if (errs.IsNotEmpty())
                throw new AggregateException(errs);
            else
                Status.Value = ReviewStatus.Completed;
        }

        private string createPointParagraph(MyIssue parent, IdentifiableName pointTracker, AssigneeModel reviewer)
        {
            var detectionProcess = Target.Process.GetQueryString();
            var saveReviewer = reviewer != null ? SettingsModel.Default.CreateTicket.SaveReviewer.GetQueryString(reviewer.Id.ToString()) : null;
            var reviewMethod = Requests.Period.GetQueryString();
            var cfQueries = Requests.CustomFields.Point.GetIssueCustomFieldQuries();
            var createPointUrl = RedmineManager.Default.Value.CreatePointIssueURL(parent.RawIssue, pointTracker.Id, detectionProcess, saveReviewer, reviewMethod, cfQueries);
            var createLink = CacheManager.Default.MarkupLang.CreateLink(Resources.ReviewMsgAddPointAtHere, createPointUrl);
            if (SettingsModel.Default.CreateTicket.SaveReviewer.IsEnabled)
            {
                var setReviewerMsg = reviewer != null ?
                    string.Format(Resources.ReviewMsgPointer, SettingsModel.Default.CreateTicket.SaveReviewer.CustomField.Name, reviewer.Name) :
                    string.Format(Resources.ReviewMsgPointerSet, SettingsModel.Default.CreateTicket.SaveReviewer.CustomField.Name);
                return CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewPoints, createLink, setReviewerMsg);
            }
            else
            {
                return CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewPoints, createLink);
            }
        }

        private async Task<Issue> createRequestTicketAsync(AssigneeModel reviewer, MyIssue openTicket, IdentifiableName requestTracker, IdentifiableName pointTracker, string description, bool isSelfReview)
        {
            var c = openTicket.CreateChildTicket();
            c.Author = CacheManager.Default.MyUser.ToIdentifiableName();
            c.Tracker = requestTracker;
            c.Status = SettingsModel.Default.CreateTicket.DefaultStatus.ToIdentifiableName();
            c.AssignedTo = reviewer.ToIdentifiableName();
            c.Subject = $"{Requests.Title.Value} {reviewer.GetPostFix(isSelfReview)}";
            c.CustomFields = createCustomFields(Requests.CustomFields.Request.GetIssueCustomFields(), reviewer);
            c.Description = string.Format(description, createPointParagraph(openTicket, pointTracker, reviewer));
            return await Task.Run(() => RedmineManager.Default.Value.CreateTicket(c));
        }

        private ApplicationException createOutlookAppointment(string subject, string body, Action<AppointmentItem> previewDisplay)
        {
            NetOffice.OutlookApi.Application outlook = null;
            try
            {
                outlook = new NetOffice.OutlookApi.Application().AddTo(disposables);
            }
            catch (System.Exception e)
            {
                return new ApplicationException(Resources.ReviewErrMsgFailedWorkWithOutlook, e);
            }

            var item = outlook.CreateItem(OlItemType.olAppointmentItem) as AppointmentItem;
            item.Start = Requests.Period.StartDateTime.Value;
            item.End = Requests.Period.DueDateTime.Value;
            item.AllDayEvent = false;
            item.Subject = subject;
            item.Body = body;

            foreach (var r in Requests.Assignee.SelectedAssignees)
            {
                var user = CacheManager.Default.Users.FirstOrDefault(a => a.Id == r.Model.Id);
                var rcp = item.Recipients.Add(user != null ? user.Email : r.Model.Name);
                rcp.Type = r.GetRecipientType();
            }
            item.Recipients.ResolveAll();

            previewDisplay.Invoke(item);

            try
            {
                item.Display();
            }
            catch (System.Exception e)
            {
                // 確認ダイアログなどが開いていると予定が開けず例外になる
                return new ApplicationException(Resources.ReviewErrMsgFailedOpenAppointment, e);
            }

            return null;
        }

        protected override List<IssueCustomField> createCustomFields(List<IssueCustomField> fields, AssigneeModel reviewer = null)
        {
            var proc = Target.Process.GetIssueCustomField();
            if (proc != null)
                fields.Add(proc);

            var face = Requests.Period.GetIssueCustomField();
            if (face != null)
                fields.Add(face);

            return base.createCustomFields(fields, reviewer);
        }

        public void ApplyTemplate(TemplateModel template)
        {
            if (Target.Ticket.Value == null)
            {
                Target.TicketNo.Value = template.Target.Ticket.Id.ToString();
                // チケットの取得に失敗したら後続の処理を行わない
                if (Target.Ticket.Value == null)
                    return;
            }

            Target.ApplyTemplate(template.Target);
            Requests.ApplyTemplate(template.Requests);
        }
    }
}
