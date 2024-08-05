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

namespace RedmineTimePuncher.ViewModels.CreateTicket
{
    public class CreateTicketViewModel : FunctionViewModelBase
    {
        [JsonIgnore]
        public BusyTextNotifier IsBusy { get; set; }

        public CreateTicketMode CreateMode { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<List<IssueStatus>> Statuss { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<string> TicketNo { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> TicketTitle { get; set; }

        public MyIssue Ticket { get; set; }
        public MemberViewModel Organizer { get; set; }
        public IssueStatus StatusUnderReview { get; set; }
        public DetectionProcessViewModel DetectionProcess { get; set; }
        public ReviewMethodViewModel ReviewMethod { get; set; }

        public bool NeedsCreateOutlookAppointment { get; set; }
        public bool NeedsGitIntegration { get; set; }
        public string MergeRequestUrl { get; set; }

        [JsonIgnore]
        public ReviewersTwinListViewModel ReviewerTwinList { get; set; }
        public ObservableCollection<MemberViewModel> Reviewers { get; set; }
        public List<MemberViewModel> AllReviewers { get; set; }

        public string ReviewTarget { get; set; }
        public string RawTitle { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public ReviewersTwinListViewModel OperatorTwinList { get; set; }
        public ObservableCollection<MemberViewModel> Operators { get; set; }
        public List<MemberViewModel> AllOperators { get; set; }

        [JsonIgnore]
        public ReactiveCommand GoToTicketCommand { get; set; }

        [JsonIgnore]
        public AsyncCommandBase OrgnizeReviewCommand { get; set; }

        [JsonIgnore]
        public CommandBase AdjustScheduleCommand { get; set; }

        public CreateTicketViewModel() { }

        public CreateTicketViewModel(MainWindowViewModel parent) : base(ApplicationMode.TicketCreater, parent)
        {
            IsBusy = new BusyTextNotifier();

            CreateTicketViewModel prev = null;
            var json = Properties.Settings.Default.CreateTicket;
            if (!string.IsNullOrEmpty(json))
            {
                prev = CloneExtentions.ToObject<CreateTicketViewModel>(json);
                CreateMode = prev.CreateMode;
            }

            ErrorMessage = IsSelected.CombineLatest(parent.Redmine, parent.ObserveProperty(a => a.Settings.CreateTicket), (isSelected, r, s) => (isSelected, r, s)).Select(t =>
            {
                // RedmineManager のチェックは MainWindowViewModel で行っているのでスルーする
                if (!t.isSelected || t.r == null)
                    return null;

                if (!t.r.CanUseAdminApiKey())
                    return Resources.ReviewErrMsgNeedAdminAPIKey;
                else if (!t.s.IsValid())
                    return Resources.ReviewErrMsgSetUpReview;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            var onTicketUpdated = this.ObserveProperty(a => Ticket).ToReactiveProperty().AddTo(disposables);

            // チケット番号
            TicketNo = new ReactivePropertySlim<string>(prev != null && prev.Ticket != null ? prev.Ticket.Id.ToString() : null).AddTo(disposables);
            TicketNo.CombineLatest(parent.Redmine, (no, r) => (no, r)).Where(p => p.no != null && p.r != null).SubscribeWithErr(p =>
            {
                var no = p.no.Trim().TrimStart('#');
                if (!Regex.IsMatch(no, "^[0-9]+$"))
                {
                    Ticket = null;
                    return;
                }

                MyIssue ticket = null;
                try
                {
                    ticket = parent.Redmine.Value.GetTicketsById(no);
                }
                catch
                {
                    Ticket = null;
                    return;
                }

                if (CacheManager.Default.IsMyProject(ticket.Project.Id))
                {
                    Ticket = ticket;
                }
                else
                {
                    Ticket = null;
                    throw new ApplicationException(Resources.ReviewErrMsgNotAssignedProject);
                }
            });
            TicketTitle = onTicketUpdated.CombineLatest(TicketNo, (t, no) =>
            {
                if (string.IsNullOrEmpty(no))
                    return null;
                else
                    return t != null ? t.Subject : Resources.ReviewErrMsgFailedToGetIssue;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Reviewers = new ObservableCollection<MemberViewModel>();
            Operators = new ObservableCollection<MemberViewModel>();

            // 開催中ステータス
            Statuss = CacheManager.Default.Updated.Select(_ => CacheManager.Default.Statuss).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.ObserveProperty(a => MergeRequestUrl).Pairwise().Subscribe(p =>
            {
                if (!NeedsGitIntegration || ReviewTarget == null || string.IsNullOrEmpty(p.NewItem))
                    return;

                if (!string.IsNullOrEmpty(p.OldItem) && ReviewTarget.Contains(p.OldItem))
                {
                    ReviewTarget = ReviewTarget.Replace(p.OldItem, p.NewItem);
                }
                else
                {
                    var label = p.NewItem.Contains("gitlab") ? "Merge Request URL" : "Pull Request URL";
                    var link = CacheManager.Default.MarkupLang.CreateLink(label, p.NewItem);
                    ReviewTarget = ReviewTarget + $"{Environment.NewLine}{Environment.NewLine}{link}";
                }
            });

            DetectionProcess = new DetectionProcessViewModel().AddTo(disposables);
            ReviewMethod = new ReviewMethodViewModel(parent.Settings).AddTo(disposables);

            // キャッシュおよびチケット更新時の処理
            // キャッシュの新規取得が実行された場合、コンストラクタでは前回値を反映できないためフラグで処理する
            var needsRestorePrev = true;
            try
            {
                needsRestorePrev = updateTicket(parent.Settings, prev, Ticket, true);
            }
            catch (System.Exception e)
            {
                Logger.Warn(e, $"Failed to updateTicket");
            }
            parent.Redmine.CombineLatest(onTicketUpdated, CacheManager.Default.Updated, (r, t, _) => (r, t)).Skip(1).SubscribeWithErr(p =>
            {
                if (p.r == null || p.t == null || !parent.Settings.CreateTicket.IsValid() || !p.r.CanUseAdminApiKey())
                    return;

                if (needsRestorePrev)
                    needsRestorePrev = updateTicket(parent.Settings, prev, p.t, needsRestorePrev);
                else
                    updateTicket(parent.Settings, this, p.t, needsRestorePrev);
            }).AddTo(disposables);

            // 設定更新時の処理
            updateSettings(parent.Settings, prev);
            parent.ObserveProperty(a => a.Settings.CreateTicket).CombineLatest(
                parent.ObserveProperty(a => a.Settings.TranscribeSettings),
                parent.ObserveProperty(a => a.Settings.RequestWork), (_1, _2, _3) => (_1, _2, _3)).Skip(1).SubscribeWithErr(c =>
                {
                    updateSettings(parent.Settings, this);
                }).AddTo(disposables);

            // タイトル
            onTicketUpdated.CombineLatest(this.ObserveProperty(a => a.DetectionProcess),
                                          this.ObserveProperty(a => a.DetectionProcess.Model.Value), (t, d, v) => (t, d, v))
                .Where(a => a.t != null).SubscribeWithErr(p =>
            {
                var processName = (p.d.Model.IsEnabled && p.v != null) ? p.v.Label : "";
                RawTitle = RawTitle == null && prev != null && !string.IsNullOrEmpty(prev.RawTitle) ?
                           prev.RawTitle :
                           $"{processName}{Resources.Review} : {p.t.Subject}";
            }).AddTo(disposables);

            GoToTicketCommand = new[] { IsBusy.Inverse(), onTicketUpdated.Select(t => t != null) }.CombineLatestValuesAreAllTrue()
                .ToReactiveCommand()
                .WithSubscribe(() => Ticket.GoToTicket()).AddTo(disposables);

            var canCreateReviewTicket = new[] {
                    IsBusy.Select(i => i ? "" : null),
                    onTicketUpdated.Select(t =>
                    {
                         if (t == null)
                            return Resources.ReviewErrMsgSelectTargetTicket;
                         else if (t.IsClosed)
                            return Resources.ReviewErrMsgTargetClosed;
                         else
                            return null;
                    }),
                    ReviewMethod.IsValidDuration,
                    this.ObserveProperty(a => a.CreateMode).StartWithDefault().CombineLatest(
                        this.ObserveProperty(a => a.Organizer).StartWithDefault(),
                        Reviewers.CollectionChangedAsObservable().StartWithDefault(),
                        Operators.CollectionChangedAsObservable().StartWithDefault(), (_1, _2, _3, _4) => true).Select(_ =>
                       {
                           if (CreateMode == CreateTicketMode.Review)
                           {
                               if (Organizer == null)
                                   return Resources.ReviewErrMsgSelectOrganizer;

                               return Reviewers.Any() ? null : Resources.ReviewErrMsgSelectReviewer;
                           }
                           else
                           {
                               return Operators.Any() ? null : Resources.ReviewErrMsgSelectOperator;
                           }
                       }),
                }.CombineLatestFirstOrDefault(a => a != null).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            OrgnizeReviewCommand = new AsyncCommandBase(
                Resources.RibbonCmdCreateReviewTicket, Resources.icons8_review,
                canCreateReviewTicket,
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(OrgnizeReviewCommand));
                    using (IsBusy.ProcessStart(Resources.ProgressMsgCreatingIssues))
                    {
                        if (CreateMode == CreateTicketMode.Review)
                            await createReviewTicketAsync(parent.Redmine.Value, parent.Settings);
                        else if (CreateMode == CreateTicketMode.Work)
                            await createRequestsTicketAsync(parent.Redmine.Value, parent.Settings);
                    }
                }).AddTo(disposables);

            AdjustScheduleCommand = new CommandBase(
                Resources.RibbonCmdAdjustSchedule, Resources.icons8_calendar_48_mod,
                canCreateReviewTicket,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(AdjustScheduleCommand));
                    var exception = createOutlookAppointment(
                        $"({Resources.ReviewForShcheduleAdjust}) {RawTitle}",
                        string.Format(Resources.ReviewMsgForAdjustSchedule, ApplicationInfo.Title),
                        i =>
                        {
                            i.CloseEvent += (ref bool cancel) =>
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ReviewMethod.StartDateTime = i.Start;
                                    ReviewMethod.DueDateTime = i.End;
                                });
                            };
                        });
                    if (exception != null)
                        throw exception;
                }).AddTo(disposables);
        }

        private CompositeDisposable myDisposables { get; set; }
        private bool updateTicket(SettingsModel settings, CreateTicketViewModel prev, MyIssue target, bool needsRestorePrev)
        {
            if (target == null)
            {
                if (prev != null && prev.Ticket != null)
                    target = prev.Ticket;
                else
                    return needsRestorePrev;
            }

            if (!CacheManager.Default.IsExist())
                return needsRestorePrev;

            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            var memberships = CacheManager.Default.ProjectMemberships.TryGetValue(target.Project.Id, out var ms) ? ms : new List<ProjectMembership>();
            AllReviewers = memberships.Select(m => new MemberViewModel(m.User, settings.CreateTicket.IsRequired)).ToList();
            AllOperators = memberships.Select(m => new MemberViewModel(m.User, settings.CreateTicket.IsRequired)).ToList();
            if (!AllReviewers.Any() || !AllOperators.Any())
                throw new ApplicationException(Resources.ReviewErrMsgNoMemberAssgined);

            if (prev != null)
            {
                // 開催者
                if (needsRestorePrev && AllReviewers.Any(m => m.User.Id == prev.Organizer?.User.Id))
                    Organizer = AllReviewers.First(m => m.User.Id == prev.Organizer?.User.Id);
                else
                    Organizer = AllReviewers.FirstOrDefault(m => m.User.Id == target.AssignedTo.Id, null);

                // レビューア
                var prevReviewers = prev.Reviewers.ToList();
                Reviewers.Clear();
                ReviewerTwinList = new ReviewersTwinListViewModel(AllReviewers, Reviewers, prevReviewers).AddTo(myDisposables);

                // 作業者
                var prevOperators = prev.Reviewers.ToList();
                Operators.Clear();
                OperatorTwinList = new ReviewersTwinListViewModel(AllOperators, Operators, prevOperators).AddTo(myDisposables);

                // 期日、開始日
                ReviewMethod.SetPreviousDuration(prev.ReviewMethod);
                NeedsCreateOutlookAppointment = prev.NeedsCreateOutlookAppointment;

                // レビュー対象
                MergeRequestUrl = needsRestorePrev ? prev.MergeRequestUrl : "";
                ReviewTarget = needsRestorePrev ? prev.ReviewTarget : CacheManager.Default.MarkupLang.CreateTicketLink(target);

                // 説明
                Description = needsRestorePrev ? prev.Description : "";

                // 開催中ステータス
                StatusUnderReview = prev.StatusUnderReview ?? CacheManager.Default.Statuss.FirstOrDefault();

                // カスタムフィールドの設定への反映
                ReviewMethod.Model?.ApplyCustomField();
                DetectionProcess.Model?.ApplyCustomField();
            }
            else
            {
                Organizer = AllReviewers.FirstOrDefault(m => m.User.Id == target.AssignedTo.Id, null);

                Reviewers.Clear();
                ReviewerTwinList = new ReviewersTwinListViewModel(AllReviewers, Reviewers).AddTo(myDisposables);

                Operators.Clear();
                OperatorTwinList = new ReviewersTwinListViewModel(AllOperators, Operators).AddTo(myDisposables);

                ReviewMethod.ResetDuration(settings);
                NeedsCreateOutlookAppointment = false;

                MergeRequestUrl = "";
                ReviewTarget = CacheManager.Default.MarkupLang.CreateTicketLink(target);

                Description = "";

                StatusUnderReview = CacheManager.Default.Statuss.FirstOrDefault();

                ReviewMethod.Model?.ApplyCustomField();
                DetectionProcess.Model?.ApplyCustomField();
            }

            return false;
        }

        private CompositeDisposable myDisposables2 { get; set; }
        private void updateSettings(SettingsModel settings, CreateTicketViewModel prev)
        {
            myDisposables2?.Dispose();
            myDisposables2 = new CompositeDisposable().AddTo(disposables);

            NeedsGitIntegration = settings.CreateTicket.NeedsGitIntegration;

            // 検出工程
            DetectionProcess.Setup(settings);
            if (prev != null)
                DetectionProcess.SetPreviousModel(prev.DetectionProcess?.Model);

            // レビュースタイル
            ReviewMethod.Setup(settings);
            if (prev != null)
                ReviewMethod.SetPreviousModel(prev.ReviewMethod?.Model);
        }

        private async Task createReviewTicketAsync(RedmineManager redmine,SettingsModel settings)
        {
            // 説明の転記機能のチェック
            var openTransPrg = "";
            var requestTransPrg = "";
            try
            {
                openTransPrg = MarkupLangType.None.CreateParagraph(null, await transcribeAsync(settings.TranscribeSettings.OpenTranscribe, redmine));
                requestTransPrg = MarkupLangType.None.CreateParagraph(null, await transcribeAsync(settings.TranscribeSettings.RequestTranscribe, redmine));
            }
            catch (ApplicationException e)
            {
                var r = MessageBoxHelper.ConfirmWarning(string.Format(Resources.ReviewErrMsgFailedTranscribeDescription, e.Message), MessageBoxHelper.ButtonType.OkCancel);
                if (!r.HasValue || !r.Value)
                    return;
            }

            // 設定のトラッカーが選択中のチケットのプロジェクトで有効かどうかのチェック
            var curProj = CacheManager.Default.Projects.First(proj => proj.Id == Ticket.Project.Id);

            var disableTrackers = new List<(string TrackerName, string TicketType)>();
            if (!settings.CreateTicket.OpenTracker.TryGetIdNameOrDefault(curProj, Ticket.RawIssue.Tracker, out var openTracker))
                disableTrackers.Add((settings.CreateTicket.OpenTracker.Name, Resources.SettingsReviOpenTicket));
            if (!settings.CreateTicket.RequestTracker.TryGetIdNameOrDefault(curProj, Ticket.RawIssue.Tracker, out var requestTracker))
                disableTrackers.Add((settings.CreateTicket.RequestTracker.Name, Resources.SettingsReviRequestTicket));
            if (!settings.CreateTicket.PointTracker.TryGetIdNameOrDefault(curProj, Ticket.RawIssue.Tracker, out var pointTracker))
                disableTrackers.Add((settings.CreateTicket.PointTracker.Name, Resources.SettingsReviPointTicket));

            if (!confirmDisableTrackers(disableTrackers))
                return;

            // 開催チケットの作成
            var p = Ticket.CreateChildTicket();
            p.Author = CacheManager.Default.MyUser.ToIdentifiableName();
            p.AssignedTo = Organizer.ToIdentifiableName();
            p.Subject = RawTitle;
            p.Tracker = openTracker;
            p.StartDate = ReviewMethod.GetStart().GetDateOnly();
            p.DueDate = ReviewMethod.GetDue().GetDateOnly();
            p.Description = "";
            p.Status = settings.CreateTicket.OpenStatus.ToIdentifiableName();
            var copiedCfs = settings.ReviewCopyCustomFields.GetCopiedCustomFields(Ticket);
            p.CustomFields = createCustomFields(copiedCfs);

            var openTicket = new MyIssue(await Task.Run(() => redmine.CreateTicket(p)));

            // 開催チケットの説明を更新
            var detectProcPrg = DetectionProcess.CreatePrgForTicket();
            var reviewMethodPrg = ReviewMethod.CreatePrgForTicket();
            var targetPrg = CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewDelivarables, ReviewTarget.SplitLines());
            var createPointPrg = createPointParagraph(redmine, settings, openTicket, pointTracker, null);
            var showAllUrl = settings.ReviewIssueList.CreateShowAllPointIssuesUrl(redmine, openTicket.RawIssue, pointTracker.Id);
            var showAllPointsPrg = CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewPointsList, CacheManager.Default.MarkupLang.CreateLink(Resources.ReviewMsgReferPointsAtHere, showAllUrl));
            var description = createDescription(detectProcPrg, reviewMethodPrg, targetPrg, createPointPrg, showAllPointsPrg, openTransPrg);
            if (NeedsGitIntegration && !string.IsNullOrEmpty(MergeRequestUrl))
            {
                description += $"{Environment.NewLine}{Environment.NewLine}";
                description += CacheManager.Default.MarkupLang.CreateCollapse("Do not edit the followings.", $"_merge_request_url={MergeRequestUrl}");
            }
            openTicket.RawIssue.Description = description;
            await Task.Run(() => redmine.UpdateTicket(openTicket.RawIssue));

            // 依頼チケットの作成
            var c = openTicket.CreateChildTicket();
            c.Author = CacheManager.Default.MyUser.ToIdentifiableName();
            c.Tracker = requestTracker;
            c.Status = settings.CreateTicket.DefaultStatus.ToIdentifiableName();

            foreach (var r in Reviewers)
            {
                c.AssignedTo = r.ToIdentifiableName();
                c.Subject = $"{RawTitle} {r.GetPostFix()}";
                c.CustomFields = createCustomFields(copiedCfs, r);
                c.Description = createDescription(
                    string.IsNullOrEmpty(Description) ? Resources.ReviewPleaseFollwings : Description,
                    detectProcPrg,
                    reviewMethodPrg,
                    targetPrg,
                    createPointParagraph(redmine, settings, openTicket, pointTracker, r),
                    showAllPointsPrg,
                    requestTransPrg);
                await Task.Run(() => redmine.CreateTicket(c));
            }

            // Outlook への予定の追加
            System.Exception failedToCreateAppointment = null;
            if (ReviewMethod.NeedsOutlookIntegration.Value && NeedsCreateOutlookAppointment)
            {
                var createPointUrl = redmine.CreatePointIssueURL(
                                        openTicket.RawIssue,
                                        pointTracker.Id,
                                        DetectionProcess.Model.GetQueryString(),
                                        null,
                                        ReviewMethod.Model.GetQueryString(),
                                        settings.ReviewCopyCustomFields.GetCopiedCustomFieldQuries(Ticket));
                var refKey = settings.Appointment.Outlook.RefsKeywords.Split(",".ToCharArray()).FirstOrDefault();
                failedToCreateAppointment = createOutlookAppointment(
                    RawTitle,
                    createDescription(
                        string.IsNullOrEmpty(Description) ? Resources.ReviewPleaseFollwings : Description,
                        DetectionProcess.CreatePrgForOutlook(),
                        ReviewMethod.CreatePrgForOutlook(),
                        MarkupLangType.None.CreateParagraph(Resources.SettingsReviOpenTicket, openTicket.Url),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewTargetIssue, Ticket.Url),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewPointsAdd, createPointUrl),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewPointsList, showAllUrl),
                        Organizer.Name,
                        refKey != null ? $"{refKey} #{Ticket.Id}" : ""),
                    i => i.Save());
            }

            // レビュー対象チケットのステータスの更新（ステータスだけ更新したいので最新のものを取得）
            var currentTicket = redmine.GetTicketsById(Ticket.Id.ToString());
            currentTicket.RawIssue.Status = StatusUnderReview;
            await Task.Run(() => redmine.UpdateTicket(currentTicket.RawIssue));

            Process.Start(openTicket.Url);

            if (failedToCreateAppointment != null)
                throw failedToCreateAppointment;
        }

        private bool confirmDisableTrackers(List<(string TrackerName, string TicketType)> disableTrackers)
        {
            if (disableTrackers.Count == 0)
                return true;

            var tns = string.Join(", ", disableTrackers.Select(pair => pair.TrackerName));
            var tts = string.Join(", ", disableTrackers.Select(pair => pair.TicketType));
            var msg = string.Format(Resources.ReviewMsgConfirmDisableTrackers, Ticket.Project.Name, tns, tts, Ticket.RawIssue.Tracker.Name);
            var r = MessageBoxHelper.ConfirmWarning(msg, MessageBoxHelper.ButtonType.OkCancel);
            return r.HasValue && r.Value;
        }

        private string createPointParagraph(RedmineManager redmine, SettingsModel settings, MyIssue parent, IdentifiableName pointTracker, MemberViewModel reviewer)
        {
            var detectionProcess = DetectionProcess.Model.GetQueryString();
            var saveReviewer = reviewer != null ? settings.CreateTicket.SaveReviewer.GetQueryString(reviewer.Id.ToString()) : null;
            var reviewMethod = ReviewMethod.Model.GetQueryString();
            var cfQueries = settings.ReviewCopyCustomFields.GetCopiedCustomFieldQuries(Ticket);
            var createPointUrl = redmine.CreatePointIssueURL(parent.RawIssue, pointTracker.Id, detectionProcess, saveReviewer, reviewMethod, cfQueries);
            var createLink = CacheManager.Default.MarkupLang.CreateLink(Resources.ReviewMsgAddPointAtHere, createPointUrl);
            if (settings.CreateTicket.SaveReviewer.IsEnabled)
            {
                var setReviewerMsg = reviewer != null ?
                    string.Format(Resources.ReviewMsgPointer, settings.CreateTicket.SaveReviewer.CustomField.Name, reviewer.Name) :
                    string.Format(Resources.ReviewMsgPointerSet, settings.CreateTicket.SaveReviewer.CustomField.Name);
                return CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewPoints, createLink, setReviewerMsg);
            }
            else
            {
                return CacheManager.Default.MarkupLang.CreateParagraph(Resources.ReviewPoints, createLink);
            }
        }

        private string createDescription(params string[] paragraphes)
        {
            return string.Join($"{Environment.NewLine}{Environment.NewLine}", paragraphes.Where(s => !string.IsNullOrWhiteSpace(s)));
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
            item.Start = ReviewMethod.StartDateTime;
            item.End = ReviewMethod.DueDateTime;
            item.AllDayEvent = false;
            item.Subject = subject;
            item.Body = body;

            foreach (var r in Reviewers)
            {
                var user = CacheManager.Default.Users.FirstOrDefault(a => a.Id == r.Id);
                var rcp = item.Recipients.Add(user != null ? user.Email : r.Name);
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

        private async Task<string[]> transcribeAsync(TranscribeSettingModel settings, RedmineManager redmine)
        {
            if (!settings.IsEnabled || !CacheManager.Default.MarkupLang.CanTranscribe())
                return new string[] { };

            var transSetting = CreateMode == CreateTicketMode.Review ?
                settings.Items.FirstOrDefault(i => i.NeedsTranscribe(Ticket, DetectionProcess.Model.IsEnabled ? DetectionProcess.Model.Value : TranscribeSettingModel.NOT_SPECIFIED_PROCESS)) :
                settings.Items.FirstOrDefault(i => i.NeedsTranscribe(Ticket));
            if (transSetting == null)
                return new string[] { };
            if (!transSetting.IsValid())
                throw new ApplicationException(Resources.SettingsReviErrMsgInvalidTranscribeSetting);

            MyWikiPage wiki = null;
            try
            {
                wiki = await Task.Run(() => redmine.GetWikiPage(transSetting.Project.Id.ToString(), transSetting.WikiPage.Title));
            }
            catch
            {
                throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindWikiPage, transSetting.WikiPage.Title));
            }

            return wiki.GetSectionLines(CacheManager.Default.MarkupLang, transSetting.Header, transSetting.IncludesHeader).Select(l => l.Text).ToArray();
        }

        private async Task createRequestsTicketAsync(RedmineManager redmine, SettingsModel settings)
        {
            // 説明の転記機能のチェック
            var requestTransPrg = "";
            try
            {
                requestTransPrg = MarkupLangType.None.CreateParagraph(null, await transcribeAsync(settings.RequestWork.RequestTranscribe, redmine));
            }
            catch (ApplicationException e)
            {
                var r = MessageBoxHelper.ConfirmWarning(string.Format(Resources.ReviewErrMsgFailedTranscribeDescription, e.Message), MessageBoxHelper.ButtonType.OkCancel);
                if (!r.HasValue || !r.Value)
                    return;
            }

            // 設定のトラッカーが選択中のチケットのプロジェクトで有効かどうかのチェック
            var curProj = CacheManager.Default.Projects.First(proj => proj.Id == Ticket.Project.Id);
            var disableTrackers = new List<(string TrackerName, string TicketType)>();
            if (!settings.CreateTicket.RequestTracker.TryGetIdNameOrDefault(curProj, Ticket.RawIssue.Tracker, out var requestTracker))
                disableTrackers.Add((settings.CreateTicket.RequestTracker.Name, Resources.SettingsReviRequestTicket));
            if (!confirmDisableTrackers(disableTrackers))
                return;

            // 依頼チケットの作成
            var c = Ticket.CreateChildTicket();
            c.Author = CacheManager.Default.MyUser.ToIdentifiableName();
            c.Tracker = requestTracker;
            c.StartDate = ReviewMethod.StartDate;
            c.DueDate = ReviewMethod.DueDate;

            var copiedCfs = settings.ReviewCopyCustomFields.GetCopiedCustomFields(Ticket);
            foreach (var o in Operators)
            {
                c.AssignedTo = o.ToIdentifiableName();
                c.Subject = $"{Resources.AppModeTicketCreaterRequestWork} : {Ticket.Subject} {o.GetPostFix()}";
                c.CustomFields = createCustomFields(copiedCfs, o);
                c.Description = createDescription(
                    string.IsNullOrEmpty(Description) ? string.Format(Resources.ReviewMsgRequestFollowings, CacheManager.Default.MarkupLang.CreateTicketLink(Ticket)) : Description,
                    requestTransPrg);
                await Task.Run(() => redmine.CreateTicket(c));
            }

            // 作業内容チケットのステータスの更新
            Ticket.RawIssue.Status = StatusUnderReview;
            await Task.Run(() => redmine.UpdateTicket(Ticket.RawIssue));

            Process.Start(Ticket.Url);
        }

        private List<IssueCustomField> createCustomFields(List<IssueCustomField> copied, MemberViewModel reviewer = null)
        {
            var results = new List<IssueCustomField>();

            if (copied != null)
                results.AddRange(copied);

            if (DetectionProcess.Model.GetIssueCustomFieldIfNeeded(out var proc))
                results.Add(proc);

            if (ReviewMethod.Model.GetIssueCustomFieldIfNeeded(out var face))
                results.Add(face);

            if (reviewer != null && reviewer.IsRequired.GetIssueCustomFieldIfNeeded(out var requierd))
                results.Add(requierd);

            // 空のリストを CustomFields に設定すると以下の箇所で例外が発生するため、空の場合は null を返す
            // Redmine.Net.Api.Extensions.CollectionExtensions の Dump メソッド
            return results.Count > 0 ? results : null;
        }

        public override void OnWindowClosed()
        {
            Properties.Settings.Default.CreateTicket = this.ToJson();
            Properties.Settings.Default.Save();
        }
    }
}
