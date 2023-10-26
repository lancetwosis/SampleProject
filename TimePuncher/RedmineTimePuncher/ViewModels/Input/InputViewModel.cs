using LibRedminePower;
using LibRedminePower.Exceptions;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.ViewModels;
using Microsoft.WindowsAPICodePack.Taskbar;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Behaviors;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Input.Controls;
using RedmineTimePuncher.ViewModels.Input;
using RedmineTimePuncher.ViewModels.Input.Resources;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using RedmineTimePuncher.ViewModels.Settings;
using RedmineTimePuncher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;
using Telerik.Windows.Controls.TreeMap;
using RedmineTimePuncher.ViewModels.Input.Bases;
using RedmineTimePuncher.ViewModels.Bases;
using Newtonsoft.Json;
using System.Reactive.Disposables;

namespace RedmineTimePuncher.ViewModels.Input
{
    public class InputViewModel : FunctionViewModelBase
    {
        public MainWindowViewModel Parent { get; set; }

        public ReadOnlyReactivePropertySlim<string> UrlBase { get; set; }

        #region "スケジュール"
        public ReactivePropertySlim<DateTime> CurrentDate { get; set; }
        public ReactivePropertySlim<InputPeriodType> PeriodType { get; set; }
        public ReactivePropertySlim<int> ActiveViewIndex { get; set; }
        public ObservableCollection<MyAppointment> Appointments { get; set; }
        public ReactiveProperty<List<MyAppointment>> SelectedAppointments { get; set; }
        public ReadOnlyReactivePropertySlim<int> SelectedAppointmentsCount { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsSelectedAppointments { get; set; }
        public ObservableCollection<MyAppointment> DeletedAppointments { get; set; }
        public ObservableCollection<ResourceType> ResourceTypes { get; set; }
        public ResourceType MyType { get; set; }
        public ObservableCollection<TimeMarker> TimeMarkers { get; set; }
        public TimeIndicatorsCollection TimeIndicators { get; set; }
        public ReactivePropertySlim<Slot> SelectedSlot { get; set; }
        public ReadOnlyReactivePropertySlim<ScheduleSettingsModel> ScheduleSettings { get; set; }
        public ReadOnlyReactivePropertySlim<FixedTickProvider> TickLength { get; set; }
        public ReadOnlyReactivePropertySlim<TimeSpan> DayStartTime { get; set; }
        public ReadOnlyReactivePropertySlim<TimeSpan> DayEndTime { get; set; }
        public ReadOnlyReactivePropertySlim<DateTime> StartTime { get; set; }
        public ReadOnlyReactivePropertySlim<DateTime> EndTime { get; set; }
        public ReactivePropertySlim<double> MinTimeRulerExtent { get; set; }
        public ReactivePropertySlim<double> ScalingSliderValue { get; set; }
        public ObservableCollection<Slot> SpecialSlots { get; set; }
        public ReadOnlyReactivePropertySlim<Predicate<IAppointment>> AppointmentFilter { get; set; }
        // ScheduleView 左上の検索ボックス用の検索文字列
        public ReactivePropertySlim<string> SearchText { get; set; }
        public ReactiveCommand ClearSearchText { get; set; }
        #endregion

        public ReadOnlyReactiveCollection<ResourceSettingViewModel> ResourceSettingList { get; set; }
        public ReadOnlyReactivePropertySlim<Func<object, bool>> GroupFilter { get; set; }


        #region "リボン"
        public AsyncCommandBase ReloadCommand { get; }

        public CommandBase SetTodayCommand { get; }
        public CommandBase DecreaseDateCommand { get; set; }
        public CommandBase IncreaseDateCommand { get; }

        public CommandBase SelectAposCommand { get; }
        #endregion

        #region "各種イベント"
        public ReactiveCommand<AppointmentDeletedEventArgs> AppointmentDeletedCommand { get; }
        public ReactiveCommand<AppointmentEditedEventArgs> AppointmentEditedCommand { get; }
        public ReactiveCommand<MouseWheelEventArgs> PreviewMouseWheelEventCommand { get; }
        public ReactiveCommand<KeyEventArgs> PreviewKeyDwonEventCommand { get; }
        public ReactiveCommand<MouseButtonEventArgs> DoubleClickEventCommand { get; }

        #endregion

        public ReactivePropertySlim<Window> Window { get; set; }

        public MyWorksViewModel MyWorks { get; set; }
        public RedmineViewModel Redmine { get; set; }
        public OutlookTeamsViewModel OutlookTeams { get; set; }
        public MembersViewModel Members { get; set; }

        public ReportsViewModel Report { get; set; }

        public ReadOnlyReactivePropertySlim<bool> NowUpdating { get; set; }

        public List<ReadOnlyReactivePropertySlim<ReactiveTimer>> Timers { get; set; }

        private string url;
        private List<MyResourceBase> defaultResouces;
        private BusyNotifier skipLoadAppointments = new BusyNotifier();

        public InputViewModel(MainWindowViewModel parent) : base(ApplicationMode.TimePuncher, parent)
        {
            Window = new ReactivePropertySlim<Window>();

            this.Parent = parent;

            CurrentDate = new ReactivePropertySlim<DateTime>(DateTime.Today).AddTo(disposables);

            PeriodType = new ReactivePropertySlim<InputPeriodType>().AddTo(disposables);
            ActiveViewIndex = new ReactivePropertySlim<int>().AddTo(disposables);
            var nowChanging = new BusyNotifier();
            PeriodType.Skip(1).Subscribe(p =>
            {
                if (nowChanging.IsBusy) return;

                using (nowChanging.ProcessStart())
                    ActiveViewIndex.Value = (int)p;
            });
            ActiveViewIndex.Skip(1).Subscribe(i =>
            {
                if (nowChanging.IsBusy) return;
                // View で日付をダブルクリックすると「１日表示」に切り替わるため設定に反映する
                using (nowChanging.ProcessStart())
                    PeriodType.Value = (InputPeriodType)i;
            });

            Appointments = new ObservableCollection<MyAppointment>();
            DeletedAppointments = new ObservableCollection<MyAppointment>();
            BindingOperations.EnableCollectionSynchronization(Appointments, new object());  // 並列処理に対応するため
            SelectedAppointments = new ReactiveProperty<List<MyAppointment>>().AddTo(disposables);
            SelectedAppointmentsCount = SelectedAppointments.Select(a => a?.Count ?? 0).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsSelectedAppointments = SelectedAppointments.Select(a => a?.Any() ?? false).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            SelectedSlot = new ReactivePropertySlim<Slot>().AddTo(disposables);
            SearchText = new ReactivePropertySlim<string>().AddTo(disposables);
            ClearSearchText = new ReactiveCommand().WithSubscribe(() => SearchText.Value = null).AddTo(disposables);
            AppointmentFilter = SearchText.Select(a =>
            {
                SelectedAppointments.Value = new List<MyAppointment>();
                Predicate<IAppointment> result = (ap) =>
                {
                    if (!string.IsNullOrEmpty(a) && ap is MyAppointment apo)
                        return SearchPatern.Check(a, apo.Subject);
                    return true;
                };
                return result;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Parent.Settings.ObserveProperty(a => a.Redmine.UrlBase).SubscribeWithErr(a =>
            {
                MyAppointment.UrlBase = MyIssue.UrlBase = MyUser.UrlBase = a;
            }).AddTo(disposables);
            ScheduleSettings = Parent.Settings.ObserveProperty(a => a.Schedule).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var tickLength = Parent.Settings.ObserveProperty(a => a.Schedule.TickLength).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            TickLength = tickLength.Select(a => a.ToTickProvider()).ToReadOnlyReactivePropertySlim();
            tickLength.Select(a => (int)a).SubscribeWithErr(a =>
            {
                RedmineManager.TickLength = a;
                OutlookManager.TickLength = a;
                TeamsManager.TickLength = a;
            }).AddTo(disposables);
            UrlBase = Parent.Settings.ObserveProperty(a => a.Redmine.UrlBase).Where(a => !string.IsNullOrEmpty(a)).Select(u => !u.EndsWith("/") ? u + "/" : u).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            DayStartTime = Parent.Settings.ObserveProperty(a => a.Schedule.DayStartTime).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            DayEndTime = Parent.Settings.ObserveProperty(a => a.Schedule.DayStartTime).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            MinTimeRulerExtent = new ReactivePropertySlim<double>(Properties.Settings.Default.MinTimeRulerExtent).AddTo(disposables);
            ScalingSliderValue = new ReactivePropertySlim<double>(MinTimeRulerExtent.Value * 100 / Parent.Settings.Schedule.TickLength.GetDefaultMinTimeRulerExtent()).AddTo(disposables);
            ScalingSliderValue.Select(a => a).SubscribeWithErr(a =>
            {
                MinTimeRulerExtent.Value = Parent.Settings.Schedule.TickLength.GetDefaultMinTimeRulerExtent() * (a / 100);
            });
            Parent.Settings.ObserveProperty(a => a.Category.IsAutoSameName).SubscribeWithErr(a =>
            {
                MyAppointment.IsAutoSameName = a;
            }).AddTo(disposables);

            StartTime = CurrentDate.CombineLatest(PeriodType, (d, p) => p.GetStartDate(d, Parent.Settings.Calendar).Add(DayStartTime.Value)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            EndTime = CurrentDate.CombineLatest(PeriodType, (d, p) => p.GetEndDate(d, Parent.Settings.Calendar).Add(DayStartTime.Value)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            // StartTime と EndTime の値が更新されてから実行したいのでここで定義する
            PeriodType.Skip(1).Subscribe(async p => await ReloadIfNeededAsync());

            // タイムマーカーを作成する。
            TimeMarkers = new ObservableCollection<TimeMarker>();
            MyAppointment.EditMarker = new TimeMarker("Edit", Brushes.Orange);
            TimeMarkers.Add(MyAppointment.EditMarker);

            // リソース管理を作成する。
            ResourceTypes = new ObservableCollection<ResourceType>();
            MyType = new ResourceType("Source");
            ResourceTypes.Add(MyType);

            // 自分のリソースを作成する。
            MyWorks = new MyWorksViewModel(this);
            Redmine = new RedmineViewModel(this);
            OutlookTeams = new OutlookTeamsViewModel(this);

            NowUpdating = new[] { MyWorks.NowUpdating, Redmine.NowUpdating, OutlookTeams.NowUpdating, }
                .CombineLatest().Select(a => a.Any(n => n)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            defaultResouces = new MyResourceBase[] { MyWorks.Resource, Redmine.Resource, OutlookTeams.Resource }.Where(a => a != null).ToList();
            MyType.Resources.AddRange(defaultResouces);

            Members = new MembersViewModel(this);

            Report = new ReportsViewModel(this);

            // リソースからリソース表示設定リストを作成する。
            if (Properties.Settings.Default.ResourcesUnVisibles == null)
                Properties.Settings.Default.ResourcesUnVisibles = new System.Collections.Specialized.StringCollection();
            ResourceSettingList = MyType.Resources.ToReadOnlyReactiveCollection(a => new ResourceSettingViewModel(a as MyResourceBase)).AddTo(disposables);
            ResourceSettingList.CollectionChangedAsObservable().StartWithDefault().Subscribe(_ => ResourceSettingList.ToList().ForEach(r => r.SetIsLastEnabled(ResourceSettingList))).AddTo(disposables);

            GroupFilter = ResourceSettingList.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                ResourceSettingList.ObserveElementProperty(a => a.IsEnabled).StartWithDefault(), (_, __) =>
                {
                    Properties.Settings.Default.ResourcesUnVisibles = ResourceSettingList.Where(a => !a.IsEnabled).Select(a => a.DisplayName).ToStringCollection();
                    return new Func<object, bool>(obj =>
                    {
                        var res = obj as IResource;
                        return res != null ? ResourceSettingList.Where(a => a.IsEnabled).Any(a => res.DisplayName == a.DisplayName) : true;
                    });
                }).ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe).AddTo(disposables);

            // リソースからスロット情報を作成する。
            var mySlots = MyType.Resources.ToReadOnlyReactiveCollection(a => (a as MyResourceBase).Slot).AddTo(disposables);
            SpecialSlots = new ObservableCollection<Slot>(mySlots);
            mySlots.ObserveAddChanged().SubscribeWithErr(s => SpecialSlots.Add(s)).AddTo(disposables);
            mySlots.ObserveRemoveChanged().SubscribeWithErr(s => SpecialSlots.Remove(s)).AddTo(disposables);

            // リソースからインジケータ情報を作成する。
            TimeIndicators = new TimeIndicatorsCollection();
            var visibleResources = ResourceSettingList.ToFilteredReadOnlyObservableCollection(a => a.IsEnabled);
            TimeIndicators.AddRange(visibleResources.SelectMany(a => a.Resource.GetReloads().Select(b => b.Indicator)));
            visibleResources.CollectionChangedAsObservable().SubscribeWithErr(args =>
            {
                switch (args.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var res in args.NewItems.Cast<ResourceSettingViewModel>())
                        {
                            TimeIndicators.AddRange(res.Resource.GetReloads().Where(a => a != null).Select(a => a.Indicator));
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (var res in args.OldItems.Cast<ResourceSettingViewModel>())
                        {
                            foreach (var ind in res.Resource.GetReloads().Where(a => a != null).Select(b => b.Indicator))
                            {
                                try
                                {
                                    TimeIndicators.Remove(ind);
                                }
                                catch (System.Collections.Generic.KeyNotFoundException)
                                {
                                    // なぜか、キーが無いと言われるが、正しく削除できているので、この例外は無視する。
                                }
                            }
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    default:
                        break;
                }
            }).AddTo(disposables);


            #region "********* 読込コマンド ************"
            var reloadCommands = new List<ResourceUpdater>() { MyWorks.Resource.Updater, Redmine.Resource.Updater };
            if (Parent.Outlook.IsInstalled)
                reloadCommands.Add(OutlookTeams.Resource.Updater);
            if (Parent.Teams.IsInstalled)
                reloadCommands.Add(OutlookTeams.Resource.Updater2);
            // 全体の読込コマンド
            ReloadCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdReload, Properties.Resources.reload,
                Parent.Redmine.Select(a => a != null ? null : ""),
                async () =>
                {
                    Logger.Info("ReloadCommand Start");

                    Parent.Redmine.Value.ClearCash();

                    // 現在実行中のコマンドがあれば、キャンセルする。
                    await Task.WhenAll(reloadCommands.Where(a => a.CancelCommand.CanExecute()).Select(a => a.CancelCommand.ExecuteAsync()));
                    // その後、全てのコマンドを実行する。
                    var tasks = reloadCommands.Where(a => a.UpdateCommand.CanExecute()).Select(a => a.UpdateCommand.ExecuteAsync()).ToList();
                    // Report は毎回キャンセル後に実行しているため、個別のキャンセル処理は不要
                    tasks.Add(Report.UpdateAsync());
                    await Task.WhenAll(tasks);

                    Logger.Info("ReloadCommand End");
                });

            // 読み込み中の場合は、タスクバーを実行中にする。
            defaultResouces.Select(a => a.IsBusy.Skip(1)).CombineLatestValuesAreAllFalse().Inverse().SubscribeWithErr(a =>
            {
                TaskbarManager.Instance.SetProgressState(
                    a ? TaskbarProgressBarState.Indeterminate : TaskbarProgressBarState.NoProgress);
            }).AddTo(disposables);
            #endregion

            #region "********* 日付関連コマンド ************"
            // 日付選択された場合の動作を作成する。
            CurrentDate.Pairwise().SubscribeWithErr(async p =>
            {
                if (skipLoadAppointments.IsBusy || Parent.Redmine.Value == null) return;

                if (MyWorks.IsEditedApos.Value)
                {
                    var isOk = MessageBoxHelper.ConfirmQuestion(Properties.Resources.msgConfDeleteEditedAppointment);
                    if (!isOk.HasValue || !isOk.Value)
                    {
                        using (skipLoadAppointments.ProcessStart())
                        {
                            CurrentDate.Value = p.OldItem;
                        }
                        return;
                    }
                }

                DeletedAppointments.RemoveAll();

                // 日付変更時に読み込みをしていたらキャンセルする。
                await Task.WhenAll(defaultResouces.SelectMany(a => a.GetReloads()).Where(a => a.CancelCommand.CanExecute()).Select(a => a.CancelCommand.ExecuteAsync()));

                Appointments.Clear();

                if (!ReloadCommand.Command.CanExecute())
                    await ReloadCommand.Command.CanExecuteChangedAsObservable().ToReadOnlyReactivePropertySlim();
                await ReloadCommand.Command.ExecuteAsync();
            }).AddTo(disposables);

            //--- 日付関連コマンド
            SetTodayCommand = new CommandBase(
                Properties.Resources.RibbonCmdToday, Properties.Resources.today32,
                Parent.Redmine.Select(a => a != null ? null : ""),
                () => { if (CurrentDate.Value != DateTime.Today) CurrentDate.Value = getMyToday(); }).AddTo(disposables);
            DecreaseDateCommand = new CommandBase(
                Properties.Resources.RibbonCmdPreviousDay, Properties.Resources.back32,
                Parent.Redmine.Select(a => a != null ? null : ""),
                () => CurrentDate.Value = CurrentDate.Value.AddDays(-1 * PeriodType.Value.GetIntervalDays())).AddTo(disposables);
            IncreaseDateCommand = new CommandBase(
                Properties.Resources.RibbonCmdNextDay, Properties.Resources.next32,
                Parent.Redmine.Select(a => a != null ? null : ""),
                () => CurrentDate.Value = CurrentDate.Value.AddDays(PeriodType.Value.GetIntervalDays())).AddTo(disposables);
            #endregion

            #region ""********* リボン（全般） *********"

            Timers = new[] { MyWorks.Timer, Redmine.Timer, OutlookTeams.OutlookTimer, OutlookTeams.TeamsTimer }.ToList();

            var isAllMyWork = SelectedAppointments.Select(a => a != null && a.Any() && a.All(b => b.IsMyWork.Value));
            var isAllMyWork_AllMember = new[]
            {
                isAllMyWork,
                SelectedAppointments.Select(a => a != null && a.All(b => b.ApoType == AppointmentType.RedmineTimeEntryMember)),
            }.CombineLatestValuesAreAllTrue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var isAllMyWork_AllNotMember = new[]
            {
                isAllMyWork,
                SelectedAppointments.Select(a => a != null && a.All(b => b.ApoType != AppointmentType.RedmineTimeEntryMember)),
            }.CombineLatestValuesAreAllTrue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // 選択スロットの予定をすべて選択する。
            SelectAposCommand = new CommandBase(
                Properties.Resources.RibbonCmdSelectAllAppos, 'A', Properties.Resources.icons8_select_all_files_48,
                Observable.Return(Properties.Resources.RibbonCmdSelectAllApposTooltip),
                new[]
                {
                    SelectedSlot.Select(a => a == null).Select(a => a ? Properties.Resources.msgErrSelectSlot : null),
                }.CombineLatest(a => a.Where(b => b != null).FirstOrDefault()),
                () =>
                {
                    TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(SelectAposCommand) + ".Executed");

                    var slot = SelectedSlot.Value;
                    var apos = Appointments.Where(a => slot.Start <= a.Start && a.End <= slot.End && a.Resources.Contains(slot.Resources.First())).ToList();
                    SelectedAppointments.Value = apos;
                }).AddTo(disposables);
            SelectAposCommand.MenuText = Properties.Resources.ScheduleViewCmdSelectAll;

            #endregion 

            AppointmentDeletedCommand = new ReactiveCommand<AppointmentDeletedEventArgs>().WithSubscribe(e =>
            {
                var apo = e.Appointment as MyAppointment;
                if (apo.TimeEntryId > 0)
                    DeletedAppointments.Add(apo);

                if (apo.MemberAppointments.Any())
                    apo.MemberAppointments.ForEach(a => Appointments.Remove(a));
            }).AddTo(disposables);

            AppointmentEditedCommand = new ReactiveCommand<AppointmentEditedEventArgs>().WithSubscribe(e =>
            {
                var apo = e.Appointment as MyAppointment;
                var scheduleView = e.Source as RadScheduleView;
                foreach (var mem in apo.MemberAppointments)
                {
                    mem.Start = apo.Start;
                    mem.End = apo.End;
                    Appointments.Remove(mem);
                    Appointments.Add(mem);
                }
            }).AddTo(disposables);

            PreviewMouseWheelEventCommand = new ReactiveCommand<MouseWheelEventArgs>().WithSubscribe(e =>
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (e.Delta < 0)
                    {
                        if (ScalingSliderValue.Value > 10)
                            ScalingSliderValue.Value -= 10;
                    }
                    else
                    {
                        if (ScalingSliderValue.Value < 200)
                            ScalingSliderValue.Value += 10;
                    }
                    e.Handled = true;
                }
            }).AddTo(disposables);

            PreviewKeyDwonEventCommand = new ReactiveCommand<KeyEventArgs>().WithSubscribe(e =>
            {
                var t = this;
                var s = e.Source as RadScheduleView;
                switch (e.Key)
                {
                    case Key.F2:
                        var appo = SelectedAppointments.Value.FirstOrDefault();
                        if (appo != null && appo.IsMyWork.Value)
                        {
                            MyWorks.RenameCommand.Command.Execute(s);
                        }
                        else
                        {
                            e.Handled = true;
                        }
                        break;
                    default:
                        break;
                }
            }).AddTo(disposables);

            DoubleClickEventCommand = new ReactiveCommand<MouseButtonEventArgs>().WithSubscribe(e =>
            {
                var s = e.Source as RadScheduleView;
                var appo = SelectedAppointments.Value.FirstOrDefault();
                if (appo != null && appo.IsMyWork.Value)
                {
                    MyWorks.RenameCommand.Command.Execute(s);
                    return;
                }

                if (SelectedSlot.Value != null)
                {
                    var rs = SelectedSlot.Value.Resources.OfType<MyResourceBase>();
                    if (rs.Any() && rs.All(r => r.IsMyWorks()))
                    {
                        RadScheduleViewCommands.CreateInlineAppointment.Execute(null, s);
                        return;
                    }
                }

                e.Handled = true;
            }).AddTo(disposables);

            Title = parent.Redmine.CombineLatest(CurrentDate, (r, c) =>
                $"{c.ToString("yyyy/MM/dd (ddd)")}  {getTitle(r)}").ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        private DateTime getMyToday()
        {
            var now = DateTime.Now;
            if (now < DateTime.Today + Parent.Settings.Schedule.DayStartTime)
            {
                return DateTime.Today.AddDays(-1);
            }
            else
            {
                return DateTime.Today;
            }
        }

        public async Task ReloadIfNeededAsync()
        {
            if (IsSelected.Value && !skipLoadAppointments.IsBusy)
                await ReloadCommand.Command.ExecuteAsync();
        }

        public override void OnWindowLoaded(RoutedEventArgs e)
        {
            Window.Value = e.Source as Window;

            // 前回データを読み込む
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastAppointments))
            {
                try
                {
                    using (skipLoadAppointments.ProcessStart())
                    {
                        CurrentDate.Value = Properties.Settings.Default.LastSelectedDate;
                        PeriodType.Value = (InputPeriodType)Properties.Settings.Default.LastInputPeriodType;
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.LastAppointments))
                        {
                            var items = CloneExtentions.ToObject<List<MyAppointmentSave>>(Properties.Settings.Default.LastAppointments);
                            var apos = items.Select(a => (a, a.ToMyAppointment(ResourceTypes))).ToList();
                            foreach (var apo in apos)
                            {
                                var memIds = apo.a.MemberAppointmentIds;
                                if (memIds != null && memIds.Any())
                                {
                                    var memApos = memIds.Select(b => apos.First(a => a.a.UniqueId == b)).Select(a => a.Item2).ToList();
                                    apo.Item2.MemberAppointments = memApos;
                                }
                            }
                            Appointments.AddRange(apos.Select(a => a.Item2).Where(a => a != null).Where(a => StartTime.Value <= a.Start && a.End <= EndTime.Value));
                        }
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.LastDeletedAppointments))
                        {
                            DeletedAppointments.AddRange(CloneExtentions.ToObject<List<MyAppointmentSave>>(Properties.Settings.Default.LastDeletedAppointments).Select(a => a.ToMyAppointment(ResourceTypes)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Apo restore error. {ex.ToString()}");
                    CurrentDate.Value = getMyToday();
                }
            }
            else
            {
                CurrentDate.Value = getMyToday();
            }
        }

        public override void OnWindowClosing(CancelEventArgs e)
        {
            // 変更があったら保存する。
            var saveCommand = MyWorks.GetNotAsyncSaveCommand();
            if (saveCommand.CanExecute())
            {
                var r = MessageBoxHelper.ConfirmQuestion(Properties.Resources.RibbonCmdSaveMsgConfirm, MessageBoxHelper.ButtonType.YesNoCancel);
                if (!r.HasValue) e.Cancel = true;
                else if (r.Value) saveCommand.Execute();
            }
        }

        public override void OnWindowClosed()
        {
            // 読込中であれば、キャンセルする。
            defaultResouces.SelectMany(a => a.GetReloads()).Where(a => a.CancelCommand.CanExecute()).ToList().ForEach(a => a.CancelCommand.Execute());

            // 予定表の内容を保存する。
            if (Parent.Redmine.Value != null)
            {
                if (!MyWorks.IsOutputed)
                {
                    Properties.Settings.Default.LastSelectedDate = CurrentDate.Value;
                    Properties.Settings.Default.LastTimeIndicatorMyWorks = MyWorks.Resource.Updater.Indicator.DateTime;
                    Properties.Settings.Default.LastTimeIndicatorRedmine = Redmine.Resource.Updater.Indicator.DateTime;
                    if (OutlookTeams.Resource != null)
                    {
                        if (OutlookTeams.Resource.Updater != null)
                            Properties.Settings.Default.LastTimeIndicatorOutlook = OutlookTeams.Resource.Updater.Indicator.DateTime;
                        if (OutlookTeams.Resource.Updater2 != null)
                            Properties.Settings.Default.LastTimeIndicatorTeams = OutlookTeams.Resource.Updater2.Indicator.DateTime;
                    }
                    var apos = Appointments.Select(a => new MyAppointmentSave(a)).ToList();
                    Properties.Settings.Default.LastAppointments = apos.ToJson();

                    var delApos = DeletedAppointments.Select(a => new MyAppointmentSave(a)).ToList();
                    Properties.Settings.Default.LastDeletedAppointments = delApos.ToJson();
                }
                else
                {
                    // 既に出力済みならば、次回起動時は、読み込まない。
                    Properties.Settings.Default.LastAppointments = null;
                }
            }
            Properties.Settings.Default.LastInputPeriodType = (int)PeriodType.Value;

            // TicketGridViewの設定を保存
            if (Redmine.TicketList.Value != null)
            {
                var ticketGridViewPropertiesList = Redmine.TicketList.Value.Select(a => a.ColumnProperties).ToList();
                Properties.Settings.Default.TicketGridViews = JsonConvert.SerializeObject(ticketGridViewPropertiesList);
                Properties.Settings.Default.SelectedTicketGridIndex = Redmine.SelectedTicketGridIndex;
            }

            // 画面サイズの保存
            Properties.Settings.Default.MinTimeRulerExtent = MinTimeRulerExtent.Value;
            Properties.Settings.Default.Save();
            this.Dispose();
        }
    }
}