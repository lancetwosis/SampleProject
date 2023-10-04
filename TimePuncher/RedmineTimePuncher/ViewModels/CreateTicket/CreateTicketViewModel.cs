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
        public BusyNotifier IsBusy { get; set; }

        public CreateTicketMode CreateMode { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<List<IssueStatus>> Statuss { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<List<MyUser>> Users { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<string> TicketNo { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> TicketTitle { get; set; }

        public MyIssue Ticket { get; set; }
        public MemberViewModel Organizer { get; set; }
        public IssueStatus StatusUnderReview { get; set; }
        public ReviewDetectionProcessSettingModel DetectionProcess { get; set; }

        public ReviewNeedsFaceToFaceSettingModel NeedsFaceToFace { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> NeedsSelectTime { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime DueDateTime { get; set; }
        public bool NeedsCreateOutlookAppointment { get; set; }
        public bool NeedsOutlookIntegration { get; set; }


        [JsonIgnore]
        public TwinListBoxViewModel<MemberViewModel> ReviewerTwinList { get; set; }
        public ObservableCollection<MemberViewModel> Reviewers { get; set; }
        public List<MemberViewModel> AllReviewers { get; set; }

        public string ReviewTarget { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public TwinListBoxViewModel<MemberViewModel> OperatorTwinList { get; set; }
        public ObservableCollection<MemberViewModel> Operators { get; set; }
        public List<MemberViewModel> AllOperators { get; set; }


        [JsonIgnore]
        public ReactiveCommand GoToTicketCommand { get; set; }

        [JsonIgnore]
        public AsyncCommandBase OrgnizeReviewCommand { get; set; }

        [JsonIgnore]
        public AsyncCommandBase AdjustScheduleCommand { get; set; }

        public CreateTicketViewModel() { }

        public CreateTicketViewModel(MainWindowViewModel parent) : base(ApplicationMode.TicketCreater, parent)
        {
            IsBusy = new BusyNotifier();

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
                if (Regex.IsMatch(no, "^[0-9]+$"))
                {
                    try
                    {
                        Ticket = parent.Redmine.Value.GetTicketsById(no);
                    }
                    catch
                    {
                        Ticket = null;
                    }
                }
                else
                {
                    Ticket = null;
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

            var isFirst = true;
            var allUsers = new List<MyUser>();
            CompositeDisposable myDisposables = null;
            parent.Redmine.CombineLatest(onTicketUpdated, (r, t) => (r, t)).SubscribeWithErr(p =>
            {
                if (p.r == null || p.t == null || !parent.Settings.CreateTicket.IsValid())
                    return;

                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                AllReviewers = p.r.GetMemberships(p.t.Project.Id).Where(m => m.User != null)
                    .Select(m => new MemberViewModel(m.User, parent.Settings.CreateTicket.IsRequired)).ToList();
                AllOperators = p.r.GetMemberships(p.t.Project.Id).Where(m => m.User != null)
                    .Select(m => new MemberViewModel(m.User, parent.Settings.RequestWork.IsRequired)).ToList();
                if (!AllReviewers.Any() || !AllOperators.Any())
                    throw new ApplicationException(Resources.ReviewErrMsgNoMemberAssgined);

                // 開催者
                Organizer = getValue(isFirst, prev,
                    vm => AllReviewers.FirstOrDefault(m => m.User.Id == vm.Organizer?.User.Id),
                    AllReviewers.FirstOrDefault(m => m.User.Id == p.t.AssignedTo.Id, null));

                // レビューア
                Reviewers.Clear();
                ReviewerTwinList = new TwinListBoxViewModel<MemberViewModel>(AllReviewers, Reviewers).AddTo(myDisposables);
                if (isFirst && prev != null && prev.Reviewers != null)
                {
                    foreach (var pre in prev.Reviewers)
                    {
                        var reviewer = AllReviewers.FirstOrDefault(r => r.User.Id == pre.User?.Id);
                        if (reviewer != null)
                        {
                            reviewer.IsRequiredReviewer.Value = pre.IsRequired == null ?
                                                                true :
                                                                pre.IsRequired.IsTrue();
                            ReviewerTwinList.ToItems.Add(reviewer);
                        }
                    }
                }

                // 作業者
                Operators.Clear();
                OperatorTwinList = new TwinListBoxViewModel<MemberViewModel>(AllOperators, Operators).AddTo(myDisposables);
                if (isFirst && prev != null && prev.Operators != null)
                {
                    foreach (var pre in prev.Operators)
                    {
                        var operate = AllOperators.FirstOrDefault(r => r.User.Id == pre.User?.Id);
                        if (operate != null)
                        {
                            operate.IsRequiredReviewer.Value = pre.IsRequired == null ?
                                                                true :
                                                                pre.IsRequired.IsTrue();
                            OperatorTwinList.ToItems.Add(operate);
                        }
                    }
                }

                // 期日、開始日
                StartDateTime = getValue(isFirst, prev, vm => vm.StartDateTime, DateTime.Today);
                DueDateTime = getValue(isFirst, prev, vm => vm.DueDateTime, DateTime.Today.AddDays(3));
                NeedsCreateOutlookAppointment = getValue(isFirst, prev, vm => vm.NeedsCreateOutlookAppointment, false);

                // レビュー対象
                ReviewTarget = getValue(isFirst, prev, vm => vm.ReviewTarget, p.r.MarkupLang.CreateTicketLink(p.t));

                // 説明
                Description = getValue(isFirst, prev, vm => vm.Description, "");

                isFirst = false;
            }).AddTo(disposables);

            // 開催中ステータス
            Statuss = parent.Redmine.Select(r => r?.Statuss.Value).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Statuss.Where(ss => ss != null).SubscribeWithErr(ss =>
            {
                if (!ss.Any())
                    throw new ApplicationException(Resources.ReviewErrMsgNoStatusAvailable);

                StatusUnderReview = getValue(true, prev, vm => ss.FirstOrDefault(s => s.Id == vm.StatusUnderReview?.Id, ss[0]), ss.First());
            });

            parent.ObserveProperty(a => a.Settings.CreateTicket).CombineLatest(
                parent.ObserveProperty(a => a.Settings.TranscribeSettings),
                parent.ObserveProperty(a => a.Settings.RequestWork), (c, _, __) => c).SubscribeWithErr(c =>
            {
                NeedsOutlookIntegration = c.NeedsOutlookIntegration;

                // 検出工程
                DetectionProcess = c.DetectionProcess;
                if (!DetectionProcess.PossibleValues.Any())
                {
                    DetectionProcess.IsEnabled = false;
                }
                else if (isFirst && prev != null && prev.DetectionProcess?.Value != null)
                {
                    var first = DetectionProcess.PossibleValues.FirstOrDefault(v => v != null && v.Value == prev.DetectionProcess?.Value.Value);
                    if (first != null)
                        DetectionProcess.Value = first;
                }

                // レビュースタイル
                NeedsFaceToFace = c.NeedsFaceToFace;
                if (isFirst && prev != null && prev.NeedsFaceToFace?.Value != null)
                {
                    var first = NeedsFaceToFace.PossibleValues.FirstOrDefault(v => v != null && v.Value == prev.NeedsFaceToFace?.Value.Value);
                    if (first != null)
                        NeedsFaceToFace.Value = first;
                }
                NeedsFaceToFace.ObserveProperty(n => n.Value).SubscribeWithErr(v =>
                {
                    if (v != null && v.Value == MyCustomFieldPossibleValue.NO)
                    {
                        StartDateTime = StartDateTime.GetDateOnly();
                        DueDateTime = DueDateTime.GetDateOnly();
                    }
                });

                if (!isFirst)
                    onTicketUpdated.ForceNotify();
            });

            NeedsSelectTime = this.ObserveProperty(a => a.NeedsFaceToFace)
                .CombineLatest(this.ObserveProperty(a => a.NeedsFaceToFace.Value), (setting, value) => (setting, value))
                .Select(p =>
            {
                if (!p.setting.IsEnabled)
                    return false;
                else
                    return (p.value == null || p.value.Value == MyCustomFieldPossibleValue.YES) ? true : false;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // タイトル
            onTicketUpdated.CombineLatest(this.ObserveProperty(a => a.DetectionProcess),
                                          this.ObserveProperty(a => a.DetectionProcess.Value), (t, d, v) => (t, d, v))
                .Where(a => a.t != null).SubscribeWithErr(p =>
            {
                var processName = (p.d.IsEnabled && p.v != null) ? p.v.Label : "";
                Title = getValue(Title == null, prev, vm => vm.Title, $"{processName}{Resources.Review} : {p.t.Subject}");
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
                    this.ObserveProperty(a => a.StartDateTime).CombineLatest(this.ObserveProperty(a => a.DueDateTime), (s, d) => s <= d ? null : Resources.ReviewErrMsgInvalidTimePeriod),
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
                }.CombineLatestFirstOrDefault(a => a != null);

            OrgnizeReviewCommand = new AsyncCommandBase(
                Resources.RibbonCmdCreateReviewTicket, Resources.icons8_review,
                canCreateReviewTicket,
                async () =>
                {
                    if (CreateMode == CreateTicketMode.Review)
                        await createReviewTicketAsync(parent.Redmine.Value, parent.Settings);
                    else if (CreateMode == CreateTicketMode.Work)
                        createRequestsTicket(parent.Redmine.Value, parent.Settings);
                }).AddTo(disposables);

            AdjustScheduleCommand = new AsyncCommandBase(
                Resources.RibbonCmdAdjustSchedule, Resources.icons8_calendar_48_mod,
                canCreateReviewTicket,
                async () =>
                {
                    var exception = createOutlookAppointment(
                        $"({Resources.ReviewForShcheduleAdjust}) {Title}",
                        string.Format(Resources.ReviewMsgForAdjustSchedule, ApplicationInfo.Title),
                        await Task.Run(() => parent.Redmine.Value.Users.Value),
                        i =>
                        {
                            i.CloseEvent += (ref bool cancel) =>
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    StartDateTime = i.Start;
                                    DueDateTime = i.End;
                                });
                            };
                        });
                    if (exception != null)
                        throw exception;
                }).AddTo(disposables);
        }

        private DateTime getValue(bool needsRestore, CreateTicketViewModel prev, Func<CreateTicketViewModel, DateTime> getter, DateTime defaultValue)
        {
            if (needsRestore && prev != null)
            {
                var v = getter.Invoke(prev);
                if (v != default(DateTime))
                    return v;
            }

            return defaultValue;
        }

        private bool getValue(bool needsRestore, CreateTicketViewModel prev, Func<CreateTicketViewModel, bool> getter, bool defaultValue)
        {
            if (needsRestore && prev != null)
            {
                var v = getter.Invoke(prev);
                if (v != default(bool))
                    return v;
            }

            return defaultValue;
        }

        private T getValue<T>(bool needsRestore, CreateTicketViewModel prev, Func<CreateTicketViewModel, T> getter, T defaultValue) where T : class
        {
            if (needsRestore && prev != null)
            {
                var v = getter.Invoke(prev);
                if (v != null)
                    return v;
            }

            return defaultValue;
        }

        private async Task createReviewTicketAsync(RedmineManager redmine,SettingsModel settings)
        {
            var openTransPrg = "";
            var requestTransPrg = "";
            try
            {
                openTransPrg = MarkupLangType.None.CreateParagraph(null, transcribeDescription(settings.TranscribeSettings.OpenTranscribe, redmine));
                requestTransPrg = MarkupLangType.None.CreateParagraph(null, transcribeDescription(settings.TranscribeSettings.RequestTranscribe, redmine));
            }
            catch (ApplicationException e)
            {
                var r = MessageBoxHelper.ConfirmWarning(string.Format(Resources.ReviewErrMsgFailedTranscribeDescription, e.Message), MessageBoxHelper.ButtonType.OkCancel);
                if (!r.HasValue || !r.Value)
                    return;
            }

            // 開催チケットの作成
            var p = Ticket.CreateChildTicket();
            p.Author = redmine.MyUser.ToIdentifiableName();
            p.AssignedTo = Organizer.ToIdentifiableName();
            p.Subject = Title;
            p.Tracker = settings.CreateTicket.OpenTracker.GetIdentifiableNameOrDefault(Ticket.RawIssue.Tracker);
            p.StartDate = StartDateTime.GetDateOnly();
            p.DueDate = DueDateTime.GetDateOnly();
            p.Description = "";
            p.Status = settings.CreateTicket.OpenStatus.ToIdentifiableName();
            if (NeedsFaceToFace.IsEnabled && NeedsFaceToFace.NeedsSaveToCustomField)
            {
                if (NeedsFaceToFace.GetIssueCustomField(out var cf))
                {
                    p.CustomFields = new List<IssueCustomField>() { cf };
                }
            }

            var openTicket = new MyIssue(redmine.CreateTicket(p));

            var detectProcPrg = DetectionProcess.IsEnabled ? redmine.MarkupLang.CreateParagraph(Resources.ReviewTargetProcess, DetectionProcess.Value.Label) : "";
            var reviewMethodPrg = "";
            if (NeedsFaceToFace.IsEnabled)
            {
                var label = $"{NeedsFaceToFace.Value.Label}";
                if (NeedsFaceToFace.IsTrue())
                    label += $" ({StartDateTime.ToString("yyyy/MM/dd HH:mm")} - {DueDateTime.ToString("yyyy/MM/dd HH:mm")})";

                reviewMethodPrg = redmine.MarkupLang.CreateParagraph(Resources.ReviewReviewMethod, label);
            }
            var targetPrg = redmine.MarkupLang.CreateParagraph(Resources.ReviewDelivarables, ReviewTarget.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            var showAllUrl = redmine.CreateShowAllPointIssues(openTicket.RawIssue, settings.CreateTicket.PointTracker.Id);
            var showAllPointsPrg = redmine.MarkupLang.CreateParagraph(Resources.ReviewPointsList,
                string.Format(Resources.ReviewMsgReferPointsAtHere, redmine.MarkupLang.CreateLink(Resources.ReviewMsgHere, showAllUrl)));
            openTicket.RawIssue.Description = createDescription(detectProcPrg, reviewMethodPrg, targetPrg, showAllPointsPrg, openTransPrg);
            redmine.UpdateTicket(openTicket.RawIssue);

            // 依頼チケットの作成
            var c = openTicket.CreateChildTicket();
            c.Author = redmine.MyUser.ToIdentifiableName();
            c.Tracker = settings.CreateTicket.RequestTracker.GetIdentifiableNameOrDefault(Ticket.RawIssue.Tracker);
            c.Status = settings.CreateTicket.DefaultStatus.ToIdentifiableName();

            foreach (var r in Reviewers)
            {
                c.AssignedTo = r.ToIdentifiableName();
                c.Subject = $"{Title} {r.GetPostFix()}";
                c.CustomFields = r.GetCustomFieldIfNeeded();
                var createPointUrl = redmine.CreatePointIssueURL(
                                        openTicket.RawIssue,
                                        settings.CreateTicket.PointTracker.GetIdentifiableNameOrDefault(Ticket.RawIssue.Tracker).Id,
                                        DetectionProcess.GetQueryString(),
                                        settings.CreateTicket.SaveReviewer.GetQueryString(r.Id.ToString()));
                c.Description = createDescription(
                    string.IsNullOrEmpty(Description) ? Resources.ReviewPleaseFollwings : Description,
                    detectProcPrg,
                    reviewMethodPrg,
                    targetPrg,
                    redmine.MarkupLang.CreateParagraph(Resources.ReviewPoints,
                        string.Format(Resources.ReviewMsgAddPointAtHere, redmine.MarkupLang.CreateLink(Resources.ReviewMsgHere, createPointUrl))),
                    showAllPointsPrg,
                    requestTransPrg);
                redmine.CreateTicket(c);
            }

            // Outlook への予定の追加
            System.Exception failedToCreateAppointment = null;
            if (NeedsOutlookIntegration && NeedsCreateOutlookAppointment && NeedsFaceToFace.IsTrue())
            {
                var createPointUrl = redmine.CreatePointIssueURL(
                                        openTicket.RawIssue,
                                        settings.CreateTicket.PointTracker.GetIdentifiableNameOrDefault(Ticket.RawIssue.Tracker).Id,
                                        DetectionProcess.GetQueryString(),
                                        null);
                var refKey = settings.Appointment.Outlook.RefsKeywords.Split(",".ToCharArray()).FirstOrDefault();
                failedToCreateAppointment = createOutlookAppointment(
                    Title,
                    createDescription(
                        string.IsNullOrEmpty(Description) ? Resources.ReviewPleaseFollwings : Description,
                        DetectionProcess.IsEnabled ? MarkupLangType.None.CreateParagraph(Resources.ReviewTargetProcess, DetectionProcess.Value.Label) : "",
                        NeedsFaceToFace.IsEnabled ? MarkupLangType.None.CreateParagraph(Resources.ReviewReviewMethod, NeedsFaceToFace.Value.Label) : "",
                        MarkupLangType.None.CreateParagraph(Resources.SettingsReviOpenTicket, openTicket.Url),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewTargetIssue, Ticket.Url),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewPointsAdd, createPointUrl),
                        MarkupLangType.None.CreateParagraph(Resources.ReviewPointsList, showAllUrl),
                        Organizer.Name,
                        refKey != null ? $"{refKey} #{Ticket.Id}" : ""),
                    await Task.Run(() => redmine.Users.Value),
                    i => i.Save());
            }

            // レビュー対象チケットのステータスの更新（ステータスだけ更新したいので最新のものを取得）
            var currentTicket = redmine.GetTicketsById(Ticket.Id.ToString());
            currentTicket.RawIssue.Status = StatusUnderReview;
            redmine.UpdateTicket(currentTicket.RawIssue);

            Process.Start(openTicket.Url);

            if (failedToCreateAppointment != null)
                throw failedToCreateAppointment;
        }

        private string createDescription(params string[] paragraphes)
        {
            return string.Join($"{Environment.NewLine}{Environment.NewLine}", paragraphes.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private ApplicationException createOutlookAppointment(string subject, string body, List<MyUser> allUsers, Action<AppointmentItem> previewDisplay)
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
            item.Start = StartDateTime;
            item.End = DueDateTime;
            item.AllDayEvent = false;
            item.Subject = subject;
            item.Body = body;

            foreach (var r in Reviewers)
            {
                var name = r.Name;
                if (allUsers != null)
                {
                    var u = allUsers.FirstOrDefault(a => a.Id == r.Id);
                    if (u != null)
                        name = u.Email;
                }
                var rcp = item.Recipients.Add(name);
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

        private string[] transcribeDescription(TranscribeSettingModel settings, RedmineManager redmine)
        {
            if (!settings.IsEnabled || redmine.MarkupLang == MarkupLangType.None)
                return new string[] { };

            var transSetting = CreateMode == CreateTicketMode.Review ?
                settings.Items.FirstOrDefault(i => i.NeedsTranscribe(Ticket, DetectionProcess.IsEnabled ? DetectionProcess.Value : TranscribeSettingModel.NOT_SPECIFIED_PROCESS)) :
                settings.Items.FirstOrDefault(i => i.NeedsTranscribe(Ticket));
            if (transSetting == null)
                return new string[] { };
            if (!transSetting.IsValid())
                throw new ApplicationException(Resources.SettingsReviErrMsgInvalidTranscribeSetting);

            MyWikiPage wiki = null;
            try
            {
                wiki = redmine.GetWikiPage(transSetting.Project.Id.ToString(), transSetting.WikiPage.Title);
            }
            catch
            {
                throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindWikiPage, transSetting.WikiPage.Title));
            }

            return wiki.GetSectionLines(redmine.MarkupLang, transSetting.Header, transSetting.IncludesHeader).Select(l => l.Text).ToArray();
        }

        private void createRequestsTicket(RedmineManager redmine, SettingsModel settings)
        {
            var requestTransPrg = "";
            try
            {
                requestTransPrg = MarkupLangType.None.CreateParagraph(null, transcribeDescription(settings.RequestWork.RequestTranscribe, redmine));
            }
            catch (ApplicationException e)
            {
                var r = MessageBoxHelper.ConfirmWarning(string.Format(Resources.ReviewErrMsgFailedTranscribeDescription, e.Message), MessageBoxHelper.ButtonType.OkCancel);
                if (!r.HasValue || !r.Value)
                    return;
            }

            // 依頼チケットの作成
            var c = Ticket.CreateChildTicket();
            c.Author = redmine.MyUser.ToIdentifiableName();
            c.Tracker = settings.RequestWork.RequestTracker.GetIdentifiableNameOrDefault(Ticket.RawIssue.Tracker);

            foreach (var o in Operators)
            {
                c.AssignedTo = o.ToIdentifiableName();
                c.Subject = $"{Resources.AppModeTicketCreaterRequestWork} : {Ticket.Subject} {o.GetPostFix()}";
                c.CustomFields = o.GetCustomFieldIfNeeded();
                c.Description = createDescription(
                    string.IsNullOrEmpty(Description) ? string.Format(Resources.ReviewMsgRequestFollowings, redmine.MarkupLang.CreateTicketLink(Ticket)) : Description,
                    requestTransPrg);
                redmine.CreateTicket(c);
            }

            // 作業内容チケットのステータスの更新
            Ticket.RawIssue.Status = StatusUnderReview;
            redmine.UpdateTicket(Ticket.RawIssue);

            Process.Start(Ticket.Url);
        }

        public override void OnWindowClosed()
        {
            Properties.Settings.Default.CreateTicket = this.ToJson();
            Properties.Settings.Default.Save();
        }
    }
}
