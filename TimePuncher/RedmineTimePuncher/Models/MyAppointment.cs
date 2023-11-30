using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Input.Resources;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Models
{
    public class MyAppointment : Appointment, IPeriod
    {
        public static bool IsAutoSameName { get; set; }
        public static ReactivePropertySlim<AppointmentColorType> ColorType { get; set; } = new ReactivePropertySlim<AppointmentColorType>();
        public static TimeMarker EditMarker { get; set; }
        public static RedmineManager Redmine { get; set; }
        public static ReactivePropertySlim<List<MyCategory>> AllCategories { get; set; } = new ReactivePropertySlim<List<MyCategory>>();

        public static string UrlBase { get; set; }

        private string ticketNo;
        public string TicketNo
        {
            get
            {
                return this.Storage<MyAppointment>().ticketNo;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.ticketNo != value)
                {
                    storage.ticketNo = value;
                    this.OnPropertyChanged(() => this.TicketNo);
                }
            }
        }

        private MyIssue ticket;
        public MyIssue Ticket
        {
            get
            {
                return this.Storage<MyAppointment>().ticket;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.ticket != value)
                {
                    storage.ticket = value;
                    this.OnPropertyChanged(() => this.Ticket);
                }
            }
        }

        private TicketTreeModel ticketTree = new TicketTreeModel();
        /// <summary>
        /// 自分自身に割り当てられているチケットを含め、再帰的に取得した親チケットの階層構造を表す。
        /// 階層構造のリストである Items は親チケット→子チケットの順にソートされている。
        /// </summary>
        public TicketTreeModel TicketTree
        {
            get
            {
                return this.Storage<MyAppointment>().ticketTree;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.ticketTree != value)
                {
                    storage.ticketTree = value;
                    this.OnPropertyChanged(() => this.TicketTree);
                }
            }
        }

        private string[] attenders;
        public string[] Attenders
        {
            get
            {
                return this.Storage<MyAppointment>().attenders;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.attenders != value)
                {
                    storage.attenders = value;
                    this.OnPropertyChanged(() => this.Attenders);
                }
            }
        }

        private Enums.AppointmentType apoType;
        public Enums.AppointmentType ApoType
        {
            get
            {
                return this.Storage<MyAppointment>().apoType;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.apoType != value)
                {
                    storage.apoType = value;
                    this.OnPropertyChanged(() => this.ApoType);
                }
            }
        }

        private int timeEntryId = -1;
        public int TimeEntryId
        {
            get
            {
                return this.Storage<MyAppointment>().timeEntryId;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.timeEntryId != value)
                {
                    storage.timeEntryId = value;
                    this.OnPropertyChanged(() => this.TimeEntryId);
                }
            }
        }

        private int fromEntryId = -1;
        public int FromEntryId
        {
            get
            {
                return this.Storage<MyAppointment>().fromEntryId;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.fromEntryId != value)
                {
                    storage.fromEntryId = value;
                    this.OnPropertyChanged(() => this.FromEntryId);
                }
            }
        }

        public string TimeEntryType { get; set; }

        private List<MyAppointment> memberAppointments = new List<MyAppointment>();
        public List<MyAppointment> MemberAppointments
        {
            get
            {
                return this.Storage<MyAppointment>().memberAppointments;
            }
            set
            {
                var storage = this.Storage<MyAppointment>();
                if (storage.memberAppointments != value)
                {
                    storage.memberAppointments = value;
                    this.OnPropertyChanged(() => this.MemberAppointments);
                }
            }
        }

        public ReadOnlyReactivePropertySlim<bool> IsError { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReactiveCommand GotoTicketCommand { get; set; }
        /// <summary>
        /// プロジェクトの「時間管理」タブで有効になっている「作業分類」の一覧。
        /// </summary>
        public ReadOnlyReactivePropertySlim<List<MyCategory>> ProjectCategories { get; set; }

        public ReadOnlyReactivePropertySlim<bool> IsMyWork { get; set; }
        public ReadOnlyReactivePropertySlim<bool> CanResize { get; set; }
        public ReadOnlyReactivePropertySlim<bool> CanDelete { get; set; }
        public ReadOnlyReactivePropertySlim<bool> CanDrag { get; set; }
        public ReadOnlyReactivePropertySlim<Brush> ProjectColor { get; set; }
        public ReadOnlyReactivePropertySlim<Brush> Background { get; set; }

        public string OutlookCategories { get; set; }
        public string ProjectPostfix { get; set; }

        public string ToolTipBody
        {
            get
            {
                if (!string.IsNullOrEmpty(Body))
                    return Body.LimitRows(20);
                else if (Ticket != null)
                    return Ticket.LimitedDescription;
                else
                    return null;
            }
        }

        private ReactivePropertySlim<string> ticketNoRp = new ReactivePropertySlim<string>();
        private ReactivePropertySlim<MyIssue> ticketRp = new ReactivePropertySlim<MyIssue>();
        private ReactivePropertySlim<ICategory> categoryRp = new ReactivePropertySlim<ICategory>();
        private ReactivePropertySlim<Enums.AppointmentType> apoRp = new ReactivePropertySlim<AppointmentType>();

        private BusyNotifier disableGetTicket = new BusyNotifier();
        private BusyNotifier disableGetTicketList = new BusyNotifier();

        private CompositeDisposable disposables = new CompositeDisposable();

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(this.TicketNo))
            {
                ticketNoRp.Value = TicketNo;
                Url = UrlBase + $"issues/{TicketNo}";
            }
            else if (propertyName == nameof(Ticket))
                ticketRp.Value = Ticket;
            else if (propertyName == nameof(Category))
                categoryRp.Value = Category;
            else if (propertyName == nameof(ApoType))
                apoRp.Value = ApoType;
            else if (propertyName == nameof(End))
                ticketRp.ForceNotify(); // 終了時間を変更した場合は、作業カテゴリの自動振り分けを行う。
        }

        /// <summary>
        /// 予定の新規作成ができるように、引数なしコンストラクタを定義しておく。
        /// </summary>
        public MyAppointment()
        {
            // チケット番号が更新されたら
            ticketNoRp.Pairwise().Where(a => !disableGetTicket.IsBusy && !string.IsNullOrEmpty(a.NewItem)).SubscribeWithErr(no =>
            {
                // チケットを取得する。
                Ticket = Redmine.GetTicketIncludeJournal(no.NewItem, out var _);

                // 新規でチケットが設定されたら
                if (string.IsNullOrEmpty(no.OldItem))
                {
                    // Subject が空だった場合のみ、更新する
                    if (string.IsNullOrEmpty(Subject))
                        Subject = Ticket?.Subject;
                }
                else
                {
                    Subject = Ticket?.Subject;
                }
            }).AddTo(disposables);

            ProjectCategories = new[]
            {
                AllCategories.Select(_ => ""),                  // 「作業分類」の一覧が更新されたり
                ticketRp.Where(a => a != null).Select(_ => ""), // チケットが更新されたりしたら
            }.CombineLatest().Where(_ => Redmine != null).Select(_ =>
            {
                var proj = Redmine.Projects.Value.First(p => p.Id == ticketRp.Value.Project.Id);
                return AllCategories.Value.Where(a => proj.TimeEntryActivities.Any(b => b.Id == a.Id)).ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // チケットが変更されたら
            // ※ D＆Dで実行された場合は、終了時間が1000年になってしまい、その時間を元に、自動カテゴリ設定をしても意味がないので、処理しない。
            // ※ 終了時間が修正されたら、毎回、自動カテゴリ設定をしなおすことも考えたが、Redmine活動から予定を作成した場合、微妙な時間帯変更で、カテゴリが変わってしまうことを避けた。
            ticketRp.Where(a => !disableGetTicketList.IsBusy && a != null && End.Year > 1000).SubscribeWithErr(ticket =>
            {
                // 自分自身を含め、再帰的に取得した親チケットのリストを取得する。
                TicketTree.Items = getParentIssues(ticket.Id.ToString()).Indexed().Select(a => new TicketTreeItemModel(a.i, a.v, a.isLast)).ToList();

                // カテゴリがまだ未選択ならば自動選択をする。
                if (Category == null)
                    Category = ProjectCategories.Value.OrderBy(a => a.Model.Order).FirstOrDefault(a => a.IsMatch(TicketTree.Items.Select(b => b.Issue).ToList(), Redmine.MyUserId, IsAutoSameName));
            }).AddTo(disposables);
            //ticketRp への Subscribe では達成できなかったため以下のようにする。（TODO: いずれ整理すること）
            this.ObserveProperty(a => a.Ticket).Subscribe(t =>
            {
                ProjectPostfix = t != null ? $" - {t.Project.Name}" : null;
            }).AddTo(disposables);

            ProjectColor = this.ObserveProperty(a => a.Ticket).ObserveOnUIDispatcher().Select(_ =>
            {
                if (Ticket != null)
                    return MyProject.COLORS.GetUniqueColorById(Ticket.Project.Id) as Brush;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Background = this.ObserveProperty(a => a.Category).CombineLatest(ProjectColor, ColorType, (_, __, ___) => true)
                .ObserveOnUIDispatcher().Select(_ =>
            {
                if (ColorType.Value == AppointmentColorType.Category)
                    return Category is MyCategory mc ? mc.CategoryBrush : null;
                else
                    return ProjectColor.Value;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            GotoTicketCommand = ticketRp.Select(a => a != null).ToReactiveCommand().WithSubscribe(() => ticketRp.Value.GoToTicket()).AddTo(disposables);

            apoRp.SubscribeWithErr(a => this.TimeMarker = a == AppointmentType.Manual ? MyAppointment.EditMarker : null).AddTo(disposables);

            IsMyWork = this.Resources.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                this.Resources.OfType<MyResourceBase>().Any(a => a.IsMyWorks())).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            CanResize = IsMyWork.CombineLatest(apoRp, (im, apo) => im && (apo != AppointmentType.RedmineTimeEntryMember)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            CanDelete = IsMyWork.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            CanDrag = apoRp.Select(a => a != AppointmentType.RedmineTimeEntryMember).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            ErrorMessage = ticketRp.CombineLatest(categoryRp, IsMyWork, (ticket, cateogry, isMy) =>
            {
                if (!isMy) return null;
                if (ticket == null) return Properties.Resources.MyAppoMsgRegisterIssue;
                if (cateogry == null) return Properties.Resources.MyAppoMsgRegisterActivity;
                if (ProjectCategories.Value != null && !ProjectCategories.Value.Contains(cateogry))
                    return string.Format(Properties.Resources.MyAppoMsgCantSetActivityToThisProject, cateogry.ToString());
                return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsError = ErrorMessage.Select(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public MyAppointment(IResource resource, AppointmentType type, string subject, string body, DateTime start, DateTime end, string ticketNo) : this()
        {
            this.Subject = subject;
            this.Body = body;
            this.Start = start;
            this.End = end;
            this.Resources.Add(resource);
            this.ApoType = type;
            this.TicketNo = ticketNo;
        }

        public MyAppointment(IResource resource, AppointmentType type, string subject, string body, DateTime start, DateTime end, string ticketNo, MyIssue issue = null) : this()
        {
            this.Subject = subject;
            this.Body = body;
            this.Start = start;
            this.End = end;
            this.Resources.Add(resource);
            this.ApoType = type;
            using (disableGetTicket.ProcessStart()) this.TicketNo = ticketNo;
            this.Ticket = issue;
        }

        /// <summary>
        /// 起動時の前回の予定の復元処理用のコンストラクタ
        /// </summary>
        public MyAppointment(IResource resource, MyAppointmentSave saved) : this()
        {
            this.Subject = saved.Subject;
            this.ProjectPostfix = saved.ProjectPostfix;
            this.Body = saved.Body;
            this.Start = saved.Start;
            this.End = saved.End;
            this.Resources.Add(resource);
            this.ApoType = saved.ApoType;
            using (disableGetTicket.ProcessStart()) this.TicketNo = saved.TicketNo;
            using (disableGetTicketList.ProcessStart()) this.Ticket = saved.Issue;
            TicketTree = saved.TicketLinks;

            // ProjectCategories.Value がまだ null であるため AllCategories から設定する
            // この段階の AllCategories は JSON から復元した Settings.Category.Items になっている
            // そのため、システム作業分類のチェックが外れていると Id が異なったものになっている
            // よって、プロジェクトで有効になっている作業分類が更新されたら Category を設定しなおす
            Category = AllCategories.Value.FirstOrDefault(c => c.DisplayName == saved.CategoryName);
            ProjectCategories.Where(a => a != null).Subscribe(_ =>
            {
                Category = ProjectCategories.Value.FirstOrDefault(a => a.DisplayName == saved.CategoryName);
            }).AddTo(disposables);
        }

        public MyAppointment(IResource resource, AppointmentType type, string subject, string body, DateTime start, DateTime end, string ticketNo, MyIssue issue, int categoryId) :
            this(resource, type, subject, body, start, end, ticketNo, issue)
        {
            Category = ProjectCategories.Value.FirstOrDefault(a => a.Id == categoryId);
            // プロジェクトで有効になっている作業分類が更新されたら Category を設定しなおす
            ProjectCategories.Where(a => a != null).Subscribe(_ =>
            {
                Category = ProjectCategories.Value.FirstOrDefault(a => a.Id == categoryId);
            }).AddTo(disposables);
        }

        public MyAppointment(IResource resource, MyTimeEntry entry) : this()
        {
            var issueId = entry.Entry.Issue.Id;
            var issue = Redmine.GetTicketIncludeJournal(issueId.ToString(), out var error);

            if (string.IsNullOrEmpty(error))
                Subject = !string.IsNullOrEmpty(entry.Subject) ? entry.Subject : issue.Subject;
            else
                Subject = error;
            Body = null;
            Start = entry.Start;
            End = entry.End;
            Resources.Add(resource);
            ApoType = AppointmentType.RedmineTimeEntry;
            TicketNo = issueId.ToString();
            Category = ProjectCategories.Value.SingleOrDefault(a => a.Id == entry.ActivityId.Value);
            // プロジェクトで有効になっている作業分類が更新されたら Category を設定しなおす
            ProjectCategories.Where(a => a != null).Subscribe(_ =>
            {
                Category = ProjectCategories.Value.FirstOrDefault(a => a.Id == entry.ActivityId.Value);
            }).AddTo(disposables);
            TimeEntryId = entry.Entry.Id;
            FromEntryId = entry.FromId;
            TimeEntryType = entry.Type == Enums.TimeEntryType.OverTime ? $"({Enums.TimeEntryType.OverTime.GetDescription()})" : null ;
        }

        public MyAppointment(IResource resource, MyTimeEntry entry, MyAppointment parent) : this(resource, entry)
        {
            Body = string.Format(Properties.Resources.MyAppoMsgRegisteredCooperationFrom, entry.Entry.User.Name);
            ApoType = AppointmentType.RedmineTimeEntryMember;
            parent?.MemberAppointments.Add(this);
        }

        /// <summary>
        /// 空き時間の計算用に追加。RP の初期化などは行っていないので、他の用途では使用しないこと。
        /// </summary>
        public MyAppointment(DateTime start, DateTime end, string subject = null, string outlookCategories = null)
        {
            Subject = subject != null ? subject : "予定あり";
            Start = start;
            End = end;
            OutlookCategories = outlookCategories;
        }

        public override IAppointment Copy()
        {
            var apo = new MyAppointment();
            apo.CopyFrom(this);
            return apo;
        }

        public override void CopyFrom(IAppointment other)
        {
            var otherApo = other as MyAppointment;
            if (otherApo != null)
            {
                base.CopyFrom(otherApo);
                using (disableGetTicket.ProcessStart())
                using (disableGetTicketList.ProcessStart())
                {
                    this.TicketNo = otherApo.TicketNo;
                    this.Ticket = otherApo.Ticket;
                    this.TicketTree = otherApo.TicketTree;
                    this.Attenders = otherApo.Attenders;
                    if (otherApo.ApoType == AppointmentType.RedmineTimeEntryMember)
                    {
                        this.ApoType = otherApo.ApoType;
                    }
                    else
                    {
                        this.ApoType = AppointmentType.SkypeCall; // PropertyChangedを走らせるため
                        this.ApoType = AppointmentType.Manual;
                    }
                    this.MemberAppointments = otherApo.MemberAppointments;
                    // TimeEntryIdと、FromEntryIdは、
                    // コピー元と別の予定として扱いたいので、意図的にコピーしない。
                }
            }
        }

        public void SetSnap(FixedTickProvider fixedTick)
        {
            var timeSpan = DateTimeInterval.ConvertToTimeSpan(fixedTick.Interval);
            this.Start = snapToTimeSpan(timeSpan, this.Start);
            this.End = snapToTimeSpan(timeSpan, this.End);
        }

        private static DateTime snapToTimeSpan(TimeSpan timeSpan, DateTime timeToSnap)
        {
            var diff = timeToSnap.Ticks % timeSpan.Ticks;
            if ((timeSpan.Ticks / 2) > diff)
                return timeToSnap.AddTicks(-diff);
            else
                return timeToSnap.AddTicks(timeSpan.Ticks - diff);
        }

        private List<MyIssue> getParentIssues(string ticketNo)
        {
            var result = new List<MyIssue>();
            while (true)
            {
                var issue = Redmine.GetIssueIncludeJournal(ticketNo);
                if (issue == null) break;
                var issueSlim = Redmine.RestoreJournals(issue, End);
                result.Add(issueSlim);
                if (!issueSlim.ParentId.HasValue) break;
                ticketNo = issueSlim.ParentId.Value.ToString();
            }
            result.Reverse();
            return result;
        }

        private MyAppointment copyMod(DateTime start, DateTime end)
        {
            var newApo = this.Copy() as MyAppointment;
            newApo.Start = start;
            newApo.End = end;
            return newApo;
        }

        #region "********* 時間関連の処理 *********"
        /// <summary>
        /// 指定した範囲と重複する部分を期間として返す
        /// </summary>
        /// <param name="apo">チェックする範囲</param>
        /// <returns></returns>
        public (DateTime Start, DateTime End) Intersection(MyAppointment apo)
        {
            return intersection(apo.Start, apo.End);
        }

        /// 指定した範囲と重複する部分を期間として返す
        /// </summary>
        /// <param name="apo">チェックする範囲</param>
        /// <returns></returns>
        public (DateTime Start, DateTime End) Intersection(TermModel term)
        {
            var apoStart = Start.Date.Add(term.Start);
            var apoEnd = End.Date.Add(term.End);
            return intersection(apoStart, apoEnd);
        }

        private (DateTime Start, DateTime End) intersection(DateTime start, DateTime end)
        {
            if (Start > start)
            {
                if (End <= end)
                {
                    return (Start, End);
                }
                return (Start, end);
            }
            else
            {
                if (End <= end)
                {
                    return (start, End);
                }
                return (start, end);
            }
        }

        public List<MyAppointment> Expect((DateTime Start, DateTime End) target)
        {
            var result = new List<MyAppointment>();
            if (target.Start == this.Start)
            {
                if (target.End == this.End)
                {
                    return null;
                }
                else if (target.End < this.End)
                {
                    result.Add(this.copyMod(target.End, this.End));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (target.Start > this.Start)
            {
                if (target.End == this.End)
                {
                    result.Add(this.copyMod(this.Start, target.Start));
                }
                else if (target.End < this.End)
                {
                    result.Add(this.copyMod(this.Start, target.Start));
                    result.Add(this.copyMod(target.End, this.End));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return result;
        }

        public bool Include(TermModel term, DateTime date)
        {
            return Start <= date.Add(term.Start) && date.Add(term.End) <= End;
        }
        #endregion

        public override string ToString()
        {
            return $"({Start} - {End}) {Subject}";
        }

        public string ToCsvLine(IEnumerable<Enums.ExportItems> items)
        {
            var result = items.Select(a => a.GetString(this).Replace(",", " "));
            return string.Join(",", result);
        }

        protected override void Dispose(bool disposing)
        {
            disposables.Dispose();

            base.Dispose(disposing);
        }

        public bool IsSame(object obj)
        {
            return obj is MyAppointment appointment &&
                   Subject == appointment.Subject &&
                   Start == appointment.Start &&
                   End == appointment.End &&
                   Body == appointment.Body &&
                   TicketNo == appointment.TicketNo;
        }
    }
}
