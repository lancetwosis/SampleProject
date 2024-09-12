using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Input.Bases;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using RedmineTimePuncher.Models.Settings;
using Telerik.Windows.Controls.ScheduleView;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Applications;

namespace RedmineTimePuncher.ViewModels.Input
{
    public class MyWorksViewModel : ResourceViewModelBase<MyWorksResource>
    {
        public AsyncCommandBase SaveCommand { get; set; }
        public AsyncCommandBase ToExtToolCommand { get; set; }
        public AsyncCommandBase ToCSVCommand { get; set; }
        public CommandBase<RadScheduleView> RenameCommand { get; set; }
        public CommandBase SplitCommand { get; }
        public CommandBase AlignCommand { get; }
        public CommandBase CopyToMyWorksCommand { get; }
        public CommandBase DeleteCommand { get; }
        public CommandBase AlignEvenlyCommand { get; }
        public CommandBase CopyToNextDayCommand { get; }
        public CommandBase CopyToPreviousDayCommand { get; }

        public ReadOnlyReactivePropertySlim<ReactiveTimer> Timer { get; set; }

        public ReadOnlyReactivePropertySlim<bool> IsEditedApos { get; set; }

        public bool IsOutputed { get; set; }

        private InputViewModel parent;
        private IFilteredReadOnlyObservableCollection<MyAppointment> myWorksApos { get; set; }

        public MyWorksViewModel(InputViewModel parent) : base()
        {
            this.parent = parent;

            Resource = new MyWorksResource(parent.UrlBase);

            myWorksApos = parent.Appointments.ToFilteredReadOnlyObservableCollection(a => a.IsMyWork.Value && a.ApoType != AppointmentType.RedmineTimeEntryMember).AddTo(disposables);
            myWorksApos.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                myWorksApos.ObserveElementProperty(a => a.Start).StartWithDefault(),
                myWorksApos.ObserveElementProperty(a => a.End).StartWithDefault(),
                myWorksApos.ObserveElementProperty(a => a.TicketNo).StartWithDefault(),
                myWorksApos.ObserveElementProperty(a => a.Category).StartWithDefault(), (_1, _2, _3, _4, _5) => true).SubscribeWithErr(_ => IsOutputed = false);

            var myWorksChanged = myWorksApos.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
               myWorksApos.ObserveElementProperty(a => a.Start).StartWithDefault(),
               myWorksApos.ObserveElementProperty(a => a.End).StartWithDefault(), (_, __, ___) => _).ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe).AddTo(disposables);
            var tickLength = parent.Parent.Settings.ObserveProperty(a => a.Schedule.TickLength).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var myWorksError = parent.Parent.Settings.ObserveProperty(a => a.Schedule).CombineLatest(parent.SelectedDate, myWorksChanged, (sche, currentDate, __) =>
            {
                var targetApos = myWorksApos.Where(a => sche.Contains(currentDate, a)).ToList();
                if (targetApos.Count == 0)
                    return null;

                var terms = sche.SpecialTerms.SelectMany(a => a.Split((int)tickLength.Value)).ToList();
                var termsDateTime = terms.Select(t => t.Start >= sche.DayStartTime ? (currentDate, t) : (currentDate.AddDays(1), t));
                var errors = termsDateTime.Where(a =>
                    (a.t.ValidationType == TermInputValidationType.RequiredInput && !targetApos.Any(b => b.Include(a.t, a.Item1))) ||
                    (a.t.ValidationType == TermInputValidationType.ProhibitedInput && targetApos.Any(b => b.Include(a.t, a.Item1))));
                if (errors.Any())
                {
                    return string.Join(Environment.NewLine, errors.Select(a => a.t).GroupBy(a => a.ValidationType).Select(g => g.Merge().Select(a => a.GetMessage())).SelectMany(a => a));
                }

                // 重なりをチェック
                if (targetApos.Any(a => targetApos.Any(b => a != b && b.IntersectsWith(a))))
                {
                    return Properties.Resources.msgErrDuplicateAppointment;
                }
                return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var myWorksWarn = parent.Parent.Settings.ObserveProperty(a => a.Schedule).CombineLatest(parent.SelectedDate, myWorksChanged, (sche, currentDate, __) =>
            {
                var targetApos = myWorksApos.Where(a => sche.Contains(currentDate, a)).ToList();
                if (targetApos.Count == 0)
                    return null;

                var terms = sche.SpecialTerms.SelectMany(a => a.Split((int)tickLength.Value)).ToList();
                var termsDateTime = terms.Select(t => t.Start >= sche.DayStartTime ? (currentDate, t) : (currentDate.AddDays(1), t));
                var errors = termsDateTime.Where(a =>
                    (a.t.ValidationType == TermInputValidationType.InputWarning && targetApos.Any(b => b.Include(a.t, a.Item1))) ||
                    (a.t.ValidationType == TermInputValidationType.NotInputWarning && !targetApos.Any(b => b.Include(a.t, a.Item1))));

                if (errors.Any())
                    return string.Join(Environment.NewLine, errors.Select(a => a.t).GroupBy(a => a.ValidationType).Select(g => g.Merge().Select(a => a.GetMessage())).SelectMany(a => a));
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // 作業実績変更時は、入力禁止、重なっている時間帯の予定を削除する。
            var isDoingMyWorksChanged = new BusyNotifier();
            new[] {
                myWorksApos.ObserveAddChanged<MyAppointment>().Select(_ => ""),
                myWorksApos.ObserveElementProperty(a => a.Start).Select(_ => ""),
                myWorksApos.ObserveElementProperty(a => a.End).Select(_ => ""),
                parent.Parent.Settings.ObserveProperty(a => a.Schedule).Select(_ => ""),
            }.CombineLatest().Where(_ => !isDoingMyWorksChanged.IsBusy && parent.Parent.Redmine.Value != null).Delay(TimeSpan.FromMilliseconds(10)).ObserveOnUIDispatcher().SubscribeWithErr(_ =>
            {
                execUpdate(parent, () =>
                {
                    using (isDoingMyWorksChanged.ProcessStart())
                    {
                        //------------------------------------
                        // 入力禁止の時間帯の予定を削除する。
                        //------------------------------------
                        foreach (var item in myWorksApos.ToList())
                        {
                            var prohibitedTerms = parent.Parent.Settings.Schedule.SpecialTerms
                                .Where(a => a.ValidationType == TermInputValidationType.ProhibitedInput)
                                .Where(a => item.IntersectsWith(a)).ToList();
                            if (prohibitedTerms.Any())
                            {
                                // 予定から一旦削除する。
                                parent.Appointments.Remove(item);

                                var addItems = new List<MyAppointment>();
                                addItems.Add(item);

                                // 入力禁止エリアに予定がある場合は、削除する。
                                foreach (var term in prohibitedTerms)
                                {
                                    var targetApos = addItems.Where(a => a.IntersectsWith(term)).ToList();
                                    foreach (var targetApo in targetApos)
                                    {
                                        addItems.Remove(targetApo);

                                        // 予定から禁止領域の時間帯を削除する。
                                        var expectedApos = targetApo.Expect(targetApo.Intersection(term));
                                        if (expectedApos != null && expectedApos.Any())
                                        {
                                            foreach (var expectedApo in expectedApos.Indexed())
                                            {
                                                if (expectedApo.isFirst)
                                                    expectedApo.v.TimeEntryId = item.TimeEntryId;
                                                else
                                                    expectedApo.v.TimeEntryId = -1;
                                                addItems.Add(expectedApo.v);
                                            }
                                        }
                                        else
                                        {
                                            // 予定が削除されたので、Redmineとの同期を取る必要がある。
                                            if (targetApo.TimeEntryId > 0)
                                            {
                                                parent.DeletedAppointments.Add(targetApo);
                                            }
                                        }
                                    }
                                }

                                parent.Appointments.AddRange(addItems);
                            }
                        }

                        //------------------------------------
                        // 重複している予定を削除する。
                        //------------------------------------
                        var dupApos = myWorksApos.Where(a => myWorksApos.Any(b => a != b && b.IntersectsWith(a))).ToList();
                        if (dupApos.Count() > 1)
                        {
                            // 予定の重なりを展開する。
                            var removeApos = new List<MyAppointment>();
                            var result = new List<MyAppointment>();

                            while (true)
                            {
                                // 最小時間の予定を抽出する。
                                var apo = dupApos.OrderBy(a => a.End - a.Start).FirstOrDefault();
                                if (apo == null) break;

                                // その予定は入力確定。
                                result.Add(apo);

                                // その予定と重なっている他の予定を抽出する。
                                var targetApos = dupApos.Where(a => a != apo && apo.IntersectsWith(a)).ToList();

                                // 重複している予定は、一旦、登録済み予定から削除する。
                                removeApos.AddRange(targetApos.Concat(new[] { apo }));
                                dupApos = dupApos.Except(removeApos).ToList();

                                // 最小時間の予定と重なりを削除した予定を算出する。
                                foreach (var targetApo in targetApos)
                                {
                                    var temp = targetApo.Expect(apo.Intersection(targetApo));
                                    if (temp != null && temp.Any())
                                    {
                                        temp.FirstOrDefault().TimeEntryId = targetApo.TimeEntryId;
                                        temp.FirstOrDefault().FromEntryId = targetApo.FromEntryId;
                                        dupApos.AddRange(temp);
                                    }
                                    else
                                    {
                                        // 予定が削除されたので、Redmineとの同期を取る必要がある。
                                        if (targetApo.TimeEntryId > 0)
                                        {
                                            parent.DeletedAppointments.Add(targetApo);
                                        }
                                    }
                                }
                            }

                            parent.Appointments.RemoveAll(a => removeApos.Contains(a));
                            parent.Appointments.AddRange(result);
                        }
                    }
                });
            }).AddTo(disposables);

            Resource.Updater.SetUpdateCommand(parent.Parent.Redmine.Select(a => a != null), async (ct) =>
            {
                await execUpdateAsync(parent, async () =>
                {
                    Logger.Info("myWorksResource.SetReloadCommand Start");

                    var errorIds = new List<int>();
                    var result = await Task.Run(() => parent.Parent.Redmine.Value.GetEntryApos(Resource, parent.Members.Resources.Value, parent.StartTime.Value, parent.EndTime.Value, out errorIds), ct);

                    // 画面に表示している予定のうち、不要なものを削除
                    parent.Appointments.RemoveAll(a =>
                    {
                        // Redmineから取得した予定で置き換えるため、未編集のものはすべて削除
                        if (a.ApoType == AppointmentType.RedmineTimeEntry)
                            return true;
                        // 共同作業として登録されている予定で
                        else if (a.ApoType == AppointmentType.RedmineTimeEntryMember)
                        {
                            var parentAppo = parent.Appointments.FirstOrDefault(b => b.MemberAppointments.Contains(a));
                            // 「親予定がすでに存在しない」 or 「親予定が編集中じゃない」なら削除
                            if (parentAppo == null || parentAppo.ApoType != AppointmentType.Manual)
                                return true;
                        }
                        return false;
                    });

                    // 取得してきた予定のうち、ユーザの操作が行われていないもののみ追加
                    result = result.Where(a =>
                    {
                        // 通常の予定で
                        if (a.ApoType == AppointmentType.RedmineTimeEntry)
                        {
                            // 「画面ですでに編集済み」 or 「画面ですでに削除済み」なら
                            // ユーザの操作を上書きしてしまうため、新規追加はしない
                            if (parent.Appointments.Any(b => b.ApoType == AppointmentType.Manual && b.TimeEntryId == a.TimeEntryId) ||
                                parent.DeletedAppointments.Any(b => b.TimeEntryId == a.TimeEntryId))
                                return false;
                        }
                        else if (a.ApoType == AppointmentType.RedmineTimeEntryMember)
                        {
                            // 承認されていたら画面に追加しない
                            if (parent.Appointments.Any(b => b.ApoType == AppointmentType.Manual && b.FromEntryId == a.TimeEntryId))
                                return false;
                        }
                        return true;
                    }).ToList();
                    parent.Appointments.AddRange(result);

                    if (errorIds.Any())
                        throw new ApplicationException(string.Format(Properties.Resources.msgErrInvalidTermComments, string.Join(", ", errorIds)));

                    Logger.Info("myWorksResource.SetReloadCommand End");
                });
            });

            var manualApos = parent.Appointments.ToFilteredReadOnlyObservableCollection(a => a.IsMyWork.Value && a.ApoType == AppointmentType.Manual).AddTo(disposables);
            IsEditedApos = manualApos.AnyAsObservable().CombineLatest(parent.DeletedAppointments.AnyAsObservable(), (a, b) => (a || b)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // 登録された予定をRedmineに登録する。
            SaveCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdSave,
                w => OutputTargetTypes.Redmine.GetIconResource(w),
                Properties.Resources.RibbonCmdSaveTooltip,
                myWorksWarn,
                (new[] {
                    parent.Parent.IsBusy.Select(a => a ? "" : null),
                    parent.Parent.Redmine.Select(a => a == null ? Properties.Resources.RibbonCmdMsgNeedsRedmineSettings : null),
                    myWorksApos.AnyAsObservable(a => a.IsError.Value, a => a.IsError.Value).Select(a  => a ? Properties.Resources.RibbonCmdMsgExistsUnsavedAppointment : null),
                    myWorksError.Select(a => !string.IsNullOrEmpty(a) ? a : null),
                    IsEditedApos.Inverse().Select(a  => a ? Properties.Resources.RibbonCmdMsgNotExistUpdatedAppointment : null),
                }).CombineLatestFirstOrDefault(a => a != null),
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(SaveCommand));

                    var setFails = new List<(MyAppointment, Exception)>();
                    var delFails = new List<(MyAppointment, Exception)>();
                    var errorIds = new List<int>();

                    await execUpdateAsync(parent, async () =>
                    {
                        // 更新された予定を、Redmineに反映する。
                        parent.Parent.Redmine.Value.SetEntryApos(parent.Parent.Settings, myWorksApos.ToList(), out setFails);

                        // 削除された予定を、Redmineに反映する。
                        parent.Parent.Redmine.Value.DelEntryApos(parent.DeletedAppointments.ToList(), out delFails);
                        parent.DeletedAppointments.RemoveAll(a => !delFails.Any(b => b.Item1 == a));

                        // 予定を取りこむ
                        var result = await Task.Run(() => parent.Parent.Redmine.Value.GetEntryApos(Resource, parent.Members.Resources.Value, parent.StartTime.Value, parent.EndTime.Value, out errorIds));

                        // 予定を画面に反映させる。
                        parent.Appointments.RemoveAll(a =>
                            !setFails.Any(b => b.Item1 == a) && !delFails.Any(b => b.Item1 == a) &&
                            (a.ApoType == AppointmentType.Manual || a.ApoType == AppointmentType.RedmineTimeEntry || a.ApoType == AppointmentType.RedmineTimeEntryMember));
                        parent.Appointments.AddRange(result);
                    });

                    // 失敗した情報をメッセージ表示する。
                    if (setFails.Any() || delFails.Any())
                    {
                        var sb = new StringBuilder();
                        foreach (var fails in setFails.Concat(delFails).GroupBy(a => a.Item2.Message))
                        {
                            sb.AppendLine(fails.Key);
                            sb.AppendLine();
                            fails.Select(a => a.Item1).ToList().ForEach(a => sb.AppendLine(a.ToString()));
                        }
                        throw new ApplicationException(sb.ToString());
                    }
                    if (errorIds.Any())
                        throw new ApplicationException(string.Format(Properties.Resources.msgErrInvalidTermComments, string.Join(", ", errorIds)));
                }).AddTo(disposables);

            Timer = parent.Parent.Settings.ObserveProperty(a => a.Appointment.MyWorks)
                .Select(a => Resource.Updater.CreateAutoReloadTimer(a, SaveCommand.Command))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            var canExport = (new[]
            {
                parent.Parent.IsBusy.Select(a => a ? "" : null),
                parent.Parent.Redmine.Select(a => a == null ? Properties.Resources.RibbonCmdMsgNeedsRedmineSettings : null),
                myWorksApos.AnyAsObservable(a => a.IsError.Value, a => a.IsError.Value).Select(a  => a ? Properties.Resources.RibbonCmdMsgExistsUnsavedAppointment : null),
                myWorksError.Select(a => !string.IsNullOrEmpty(a) ? a : null),
            }).CombineLatestFirstOrDefault(a => a != null).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            // CSVエクスポートに出力する。
            ToCSVCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdCSV,
                w => OutputTargetTypes.CsvExport.GetIconResource(w),
                Properties.Resources.RibbonCmdCSVTooltip,
                myWorksWarn,
                canExport,
                async () =>
                {
                    var r = confirmExportIfNeeded();
                    if (!r.Date.HasValue)
                        return;

                    TraceHelper.TrackCommand(nameof(ToCSVCommand));

                    parent.Parent.Settings.OutputData.CsvExport.Export(r.Date.Value, r.Appointments);
                    await Task.CompletedTask;
                }).AddTo(disposables);

            // 外部ツールに登録する。
            ToExtToolCommand = new AsyncCommandBase(
                Properties.Resources.RibbonCmdExternalTool,
                w => OutputTargetTypes.ExtTool.GetIconResource(w),
                Properties.Resources.RibbonCmdExternalToolTooltip,
                myWorksWarn,
                (new[] {
                    canExport,
                    parent.Parent.Settings.ObserveProperty(a => a.OutputData.ExtTool.Error.Value),
                }).CombineLatestFirstOrDefault(a => a != null),
                async () =>
                {
                    var r = confirmExportIfNeeded();
                    if (!r.Date.HasValue)
                        return;

                    TraceHelper.TrackCommand(nameof(ToExtToolCommand));

                    parent.Parent.Settings.OutputData.ExportToExtTool(r.Date.Value, r.Appointments);
                    await Task.CompletedTask;
                }).AddTo(disposables);

            // 選択された予定を編集する。
            var selectAny = parent.SelectedAppointments.Select(a => a == null || !a.Any() ? Properties.Resources.msgErrSelectAppointments : null).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var canEdit = parent.SelectedAppointments.Select(a =>
            {
                if (a != null && a.Any(b => !b.IsActiveProject.Value))
                    return Properties.Resources.msgErrSelectAppointmentsOfSpentTime;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var notContainsCollab = parent.SelectedAppointments.Select(a =>
            {
                if (a != null && a.Any(b => b.ApoType == AppointmentType.RedmineTimeEntryMember))
                    return Properties.Resources.RibbonCmdMsgSelectNonCollaboAppointment;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            RenameCommand = new CommandBase<RadScheduleView>(
                Properties.Resources.RibbonCmdRename, 'M', Properties.Resources.icons8_rename_48,
                new[] { selectAny, canEdit, notContainsCollab }.CombineLatestFirstOrDefault(a => a != null),
                scheduleView =>
                {
                    RadScheduleViewCommands.BeginInlineEditing.Execute(null, scheduleView);
                }).AddTo(disposables);

            // 選択された予定を分割する。
            SplitCommand = new CommandBase(
                Properties.Resources.RibbonCmdSplit, Properties.Resources.icons8_cut_paper_48,
                new[] { selectAny, canEdit, notContainsCollab }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    var splitCount = MessageBoxHelper.Input(Properties.Resources.msgInputSplitNumber, 2, 2, 10);
                    if (!splitCount.HasValue)
                        return;

                    foreach (var apo in parent.SelectedAppointments.Value)
                    {
                        var range = apo.End - apo.Start;
                        var minSpan = DateTimeInterval.ConvertToTimeSpan(parent.TickLength.Value.Interval);
                        var spanBlockCount = Convert.ToInt32(Math.Floor(range.TotalMinutes / minSpan.TotalMinutes));
                        if (spanBlockCount <= 1) break;

                        var targetBlockCount = spanBlockCount / splitCount.Value;
                        if (targetBlockCount <= 0) break;

                        var resultApos = new List<MyAppointment>();

                        foreach (var i in Enumerable.Range(0, splitCount.Value).Indexed())
                        {
                            var splitedApo = new MyAppointment();
                            splitedApo.CopyFrom(apo);
                            splitedApo.Start =
                                i.isFirst ?
                                    apo.Start :
                                    resultApos.Last().End;
                            splitedApo.End =
                                i.isLast ?
                                    apo.End :
                                    splitedApo.Start + TimeSpan.FromMinutes(minSpan.TotalMinutes * targetBlockCount);
                            resultApos.Add(splitedApo);
                            if (splitedApo.End >= apo.End) break;
                        }
                        parent.Appointments.Remove(apo);
                        parent.Appointments.AddRange(resultApos);
                    }
                }).AddTo(disposables);

            // 選択された予定を整頓する
            AlignCommand = new CommandBase(
                Properties.Resources.RibbonCmdAlignment, Properties.Resources.icons8_view_headline_48,
                new[] { selectAny, canEdit, notContainsCollab }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(AlignCommand));
                    alignApos(parent.SelectedAppointments.Value);
                }).AddTo(disposables);

            // MyWorksへコピーの処理
            CopyToMyWorksCommand = new CommandBase(
                Properties.Resources.RibbonCmdCopyToSpentTime, 'C', Properties.Resources.icons8_assignment_return_48,
                new[] {
                    selectAny,
                    parent.SelectedAppointments.Select(a =>
                    {
                        if (a != null && a.Any(b => b.IsMyWork.Value || b.ProjectStatus == ProjectStatusType.NotActive))
                            return Properties.Resources.msgErrSelectAppoOtherThanSpentTime;
                        else
                            return null;
                    }),
                    notContainsCollab
                }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(CopyToMyWorksCommand));
                    var items = parent.SelectedAppointments.Value.Select(apo =>
                    {
                        var copiedApo = new MyAppointment();
                        copiedApo.CopyFrom(apo);
                        copiedApo.MemberAppointments.Clear();
                        copiedApo.Resources.Clear();
                        copiedApo.Resources.Add(Resource);
                        return copiedApo;
                    }).ToList();

                    // 最小入力単位に合わせる。
                    items.ForEach(a => a.SetSnap(parent.TickLength.Value));
                    parent.Appointments.AddRange(items);
                }).AddTo(disposables);

            DeleteCommand = new CommandBase(
                Properties.Resources.RibbonCmdDelete, 'D', Properties.Resources.remove_task,
                Properties.Resources.RibbonCmdDeleteTooltip,
                new[] { selectAny, canEdit }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(DeleteCommand));

                    if (parent.SelectedAppointments.Value.Count() > 1)
                    {
                        var r = MessageBoxHelper.ConfirmQuestion(string.Format(Properties.Resources.msgConfDeleteSelectedAppointments, parent.SelectedAppointments.Value.Count()));
                        if (!r.HasValue || !r.Value)
                            return;
                    }

                    parent.DeletedAppointments.AddRange(parent.SelectedAppointments.Value.Where(a => a.TimeEntryId > 0));
                    parent.SelectedAppointments.Value.ToList().ForEach(a => parent.Appointments.Remove(a));
                }).AddTo(disposables);

            // 選択範囲内のスロットになる予定を均等に整列
            AlignEvenlyCommand = new CommandBase(
                Properties.Resources.RibbonCmdAlignAppointments, Properties.Resources.icons8_sort_by_48,
                Properties.Resources.RibbonCmdAlignAppointmentsTooltip,
                new[]
                {
                    parent.SelectedSlot.Select(a => a == null).Select(a => a ? Properties.Resources.msgErrSelectSlot : null),
                    parent.SelectedSlot.Select(a => !a?.Resources.Contains(Resource)).Select(a => (a.HasValue && a.Value) ? Properties.Resources.msgErrSelectSlotOfSpentTime : null),
                }.CombineLatest(a => a.Where(b => b != null).FirstOrDefault()),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(AlignEvenlyCommand));

                    var slot = parent.SelectedSlot.Value;
                    var prohibitedTerms = parent.Parent.Settings.Schedule.SpecialTerms
                    .Where(a => a.ValidationType == TermInputValidationType.ProhibitedInput || a.ValidationType == TermInputValidationType.InputWarning)
                    .Where(a => slot.IntersectsWith(a)).ToList();
                    var slotCount = ((int)(slot.End - slot.Start).TotalMinutes - (prohibitedTerms.Sum(a => (a.End - a.Start).TotalMinutes))) / (int)tickLength.Value;
                    var apos = myWorksApos.Where(a => a.IntersectsWith(slot)).ToList();
                    if (!apos.Any()) return;

                    var notActive = apos.FirstOrDefault(a => a.ProjectStatus == ProjectStatusType.NotActive);
                    if (notActive != null)
                        throw new ApplicationException(string.Format(Properties.Resources.msgContainsNotActiveProject, notActive.ToString()));

                    // 予定から一旦、削除する。
                    parent.Appointments.RemoveAll(a => apos.Contains(a));

                    var result = new List<MyAppointment>();
                    var ratio = (int)slotCount / apos.Count();
                    var over = slotCount % apos.Count();
                    foreach (var apo in apos.Indexed())
                    {
                        var newApo = new MyAppointment();
                        newApo.CopyFrom(apo.v);

                        // 開始位置を検出する。
                        var start = result.Any() ? result.Last().End : slot.Start;
                        while (prohibitedTerms.Any(a => a.Contains(start)))
                        {
                            start = start.AddMinutes(1 * (int)tickLength.Value);
                        }
                        newApo.Start = start;

                        var addSlotCount = ratio + (apo.i < over ? 1 : 0);
                        var end = start;
                        while (addSlotCount > 0)
                        {
                            end = end.AddMinutes(1 * (int)tickLength.Value);
                            if (!prohibitedTerms.Any(a => a.Contains(end)))
                                addSlotCount -= 1;
                        }

                        newApo.Start = start;
                        newApo.End = end;
                        newApo.TimeEntryId = apo.v.TimeEntryId;
                        if (newApo.End != newApo.Start)
                            result.Add(newApo);
                    }

                    // 削除された予定を検出する。
                    var delApos = apos.Where(a => !result.Any(b => a.TimeEntryId == b.TimeEntryId)).ToList();
                    if (delApos.Any()) parent.DeletedAppointments.AddRange(delApos);

                    // 予定を追加する。
                    parent.Appointments.AddRange(result);
                }).AddTo(disposables);
            AlignEvenlyCommand.MenuText = Properties.Resources.ScheduleViewCmdAlignEvenly;

            var canExecCopy = parent.SelectedAppointments.Select(apos =>
            {
                if (apos == null || !apos.Any(a => a.IsMyWork.Value))
                    return Properties.Resources.RibbonCmdCopyToErrMsgSelectMyWork;
                if (apos.Any(a => !a.IsActiveProject.Value))
                    return Properties.Resources.RibbonCmdCopyToErrMsgSelectMyWorkOnly;

                var targets = apos.Where(a => a.IsMyWork.Value).ToList();
                var targetDay = targets.Last().Start.Date;
                if (targets.Any(a => a.Start.Date != targetDay))
                    return Properties.Resources.RibbonCmdCopyToErrMsgSelectSameDay;

                return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            CopyToPreviousDayCommand = new CommandBase(
                Properties.Resources.RibbonCmdCopyToPrevious, 'P', Properties.Resources.copy2back_48,
                canExecCopy,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(CopyToPreviousDayCommand));
                    copyToOtherDay(parent.SelectedDate.Value.AddDays(-1).Date, false);
                }).AddTo(disposables);
            CopyToNextDayCommand = new CommandBase(
                Properties.Resources.RibbonCmdCopyToNext, 'N', Properties.Resources.copy2next_48,
                canExecCopy,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(CopyToNextDayCommand));
                    copyToOtherDay(parent.SelectedDate.Value.AddDays(1).Date, true);
                }).AddTo(disposables);
        }

        private ReactiveCommand save;
        /// <summary>
        /// 保存処理は、非同期ではない同期コマンドも用意しておき、アプリ終了時に利用する。
        /// （本当はもう少しうまいやり方をさがしたのだが、うまくできなかった。)
        /// </summary>
        public ReactiveCommand GetNotAsyncSaveCommand()
        {
            if (save == null)
            {
                save = SaveCommand.Command.CanExecuteChangedAsObservable().StartWithDefault().Select(a => SaveCommand.Command.CanExecute()).ToReactiveCommand().WithSubscribe(() =>
                {
                    // 更新された予定を、、Redmineに反映する。
                    parent.Parent.Redmine.Value.SetEntryApos(parent.Parent.Settings, myWorksApos.ToList(), out var failList1);

                    // 削除された予定を、Redmineに反映する。
                    parent.Parent.Redmine.Value.DelEntryApos(parent.DeletedAppointments.ToList(), out var failList2);
                    parent.DeletedAppointments.RemoveAll(a => !failList2.Any(b => b.Item1 == a));

                    // 予定を取りこむ
                    var errorIds = new List<int>();
                    var result = parent.Parent.Redmine.Value.GetEntryApos(Resource, parent.Members.Resources.Value, parent.StartTime.Value, parent.EndTime.Value, out errorIds);

                    // 予定を画面に反映させる。
                    parent.Appointments.RemoveAll(a =>
                        !failList1.Any(b => b.Item1 == a) && !failList2.Any(b => b.Item1 == a) &&
                        (a.ApoType == AppointmentType.Manual || a.ApoType == AppointmentType.RedmineTimeEntry || a.ApoType == AppointmentType.RedmineTimeEntryMember));
                    parent.Appointments.AddRange(result);

                    // 失敗した情報をメッセージ表示する。
                    if (failList1.Any() || failList2.Any())
                    {
                        var sb = new StringBuilder();
                        foreach (var fails in failList1.Concat(failList2).GroupBy(a => a.Item2.Message))
                        {
                            sb.AppendLine(fails.Key);
                            sb.AppendLine();
                            fails.Select(a => a.Item1).ToList().ForEach(a => sb.AppendLine(a.ToString()));
                        }
                        throw new ApplicationException(sb.ToString());
                    }
                    if (errorIds.Any())
                        throw new ApplicationException(string.Format(Properties.Resources.msgErrInvalidTermComments, string.Join(", ", errorIds)));
                }).AddTo(disposables);
            }

            return save;
        }

        private void alignApos(List<MyAppointment> targets)
        {
            targets = targets.Where(a => a.ApoType != AppointmentType.RedmineTimeEntryMember).ToList();

            var res = targets.SelectMany(a => a.Resources).Distinct();
            var apos = parent.Appointments
                .Where(a => res.Any(b => a.Resources.Contains(b)))
                .Where(a => a.ApoType != AppointmentType.RedmineTimeEntryMember)
                .Where(a => targets.Any(b => b.IntersectsWith(a)))
                .ToList();

            // 予定の重なりを展開する。
            var removeApos = new List<MyAppointment>();
            var result = new List<MyAppointment>();
            while (true)
            {
                // 最小時間の予定を抽出する。
                var apo = apos.OrderBy(a => a.End - a.Start).FirstOrDefault();
                if (apo == null) break;

                // その予定は入力確定。
                result.Add(apo);

                // その予定と重なっている他の予定を抽出する。
                var targetApos = apos.Where(a => a != apo && apo.IntersectsWith(a));

                // 重複している予定は、登録済み予定から削除する。
                removeApos.AddRange(targetApos.Concat(new[] { apo }));
                apos = apos.Except(removeApos).ToList();

                // 最小時間の予定と重なりを削除した予定を算出する。
                foreach (var targetApo in targetApos)
                {
                    var temp = targetApo.Expect(apo.Intersection(targetApo));
                    temp.FirstOrDefault().TimeEntryId = targetApo.TimeEntryId;
                    temp.FirstOrDefault().FromEntryId = targetApo.FromEntryId;
                    apos.AddRange(temp);
                }
            }

            // 入力禁止エリアに予定がある場合は、確認後に削除する。
            var allTerms = parent.Parent.Settings.Schedule.SpecialTerms.SelectMany(a => a.Split((int)parent.Parent.Settings.Schedule.TickLength))
                .Where(a => a.ValidationType == TermInputValidationType.InputWarning || a.ValidationType == TermInputValidationType.ProhibitedInput).ToList();
            var targetTerms = allTerms
                .Where(a => result.Any(b => b.Start.TimeOfDay <= a.Start && a.End <= b.End.TimeOfDay)).ToList();
            if (targetTerms.Any())
            {
                foreach (var group in targetTerms.Merge())
                {
                    var part = group.ValidationType.GetDescription();
                    var message = string.Format(Properties.Resources.RibbonCmdAlignmentConfirm, part, $"{group.Start.ToString(@"hh\:mm")} - { group.End.ToString(@"hh\:mm")}");

                    var r = MessageBoxHelper.ConfirmQuestion(message);
                    if (r.HasValue && r.Value)
                    {
                        foreach (var term in group.Split((int)parent.Parent.Settings.Schedule.TickLength))
                        {
                            var targetApos = result.Where(a => a.Start.TimeOfDay <= term.Start && term.End <= a.End.TimeOfDay).ToList();
                            if (targetApos.Any())
                            {
                                foreach (var targetApo in targetApos)
                                {
                                    // 予定から一旦削除する。
                                    result.Remove(targetApo);

                                    // 取り除いた予定を算出して、結果に入れる。
                                    var addApos = targetApo.Expect(targetApo.Intersection(term));
                                    if (addApos != null && addApos.Any())
                                    {
                                        foreach (var addApo in addApos.Indexed())
                                        {
                                            if (addApo.isFirst)
                                                addApo.v.TimeEntryId = targetApo.TimeEntryId;
                                            else
                                                addApo.v.TimeEntryId = -1;
                                            result.Add(addApo.v);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            parent.Appointments.RemoveAll(a => removeApos.Contains(a));
            parent.Appointments.AddRange(result);
        }

        private (DateTime? Date, List<MyAppointment> Appointments) confirmExportIfNeeded()
        {
            var targetDate = parent.SelectedDate.Value;
            var targetApos = myWorksApos.Where(a => parent.Parent.Settings.Schedule.Contains(parent.SelectedDate.Value, a)).ToList();
            if (targetApos.Count == 0)
            {
                var r1 = MessageBoxHelper.ConfirmQuestion(string.Format(Properties.Resources.msgConfExportEmpty, targetDate.ToDateString()));
                if (r1.HasValue && r1.Value)
                    return (targetDate, targetApos);
                else
                    return (null, null);
            }

            if (parent.PeriodType.Value == InputPeriodType.OneDay)
                return (targetDate, targetApos);

            var r2 = MessageBoxHelper.ConfirmQuestion(string.Format(Properties.Resources.msgConfExport, targetDate.ToDateString()));
            if (r2.HasValue && r2.Value)
                return (targetDate, targetApos);
            else
                return (null, null);
        }

        private void copyToOtherDay(DateTime toDate, bool moveNext)
        {
            // コピー先の日が表示範囲外かどうか？（コピー後に表示範囲の更新が必要かどうか？）
            var needsChangeSelectedDate = toDate < parent.DisplayStartTime.Value ||  parent.DisplayEndTime.Value <= toDate;
            if (needsChangeSelectedDate)
            {
                var r = MessageBoxHelper.ConfirmQuestion(string.Format(Properties.Resources.RibbonCmdCopyToConfirmMsgDisplay, toDate.ToDateString()));
                if (!r.HasValue || !r.Value)
                    return;
            }

            // コピー先の日が休みの日かどうか？
            var isToDateHoliday = !parent.Parent.Settings.Calendar.IsWorkingDay(toDate);
            if (isToDateHoliday)
            {
                // コピー先の日を「直近の稼働日」に変更するかどうか確認する
                var recentWorkDay = parent.Parent.Settings.Calendar.GetMostRecentWorkingDay(toDate, moveNext);
                var msg = string.Format(Properties.Resources.RibbonCmdCopyToConfirmMsgHolidy, toDate.ToDateString(), recentWorkDay.ToDateString());
                var selectedIndex = MessageBoxHelper.Select(msg, toDate.ToDateString(), recentWorkDay.ToDateString());
                if (!selectedIndex.HasValue)
                    return;

                if (selectedIndex.Value == 1)
                {
                    toDate = recentWorkDay;
                    needsChangeSelectedDate = toDate < parent.DisplayStartTime.Value || parent.DisplayEndTime.Value <= toDate;
                    isToDateHoliday = false;
                }
            }

            // CopyToNextDayCommand の実行条件で SelectedAppointments には、単一の日の作業実績の予定のみが含まれるようになっている
            var targets = parent.SelectedAppointments.Value.Select(a =>
            {
                var copied = a.Copy() as MyAppointment;
                copied.Start = new DateTime(toDate.Year, toDate.Month, toDate.Day, copied.Start.Hour, copied.Start.Minute, copied.Start.Second);
                copied.End = new DateTime(toDate.Year, toDate.Month, toDate.Day, copied.End.Hour, copied.End.Minute, copied.End.Second);
                copied.MemberAppointments.Clear();
                copied.Resources.Clear();
                copied.Resources.Add(Resource);
                return copied;
            }).ToList();

            // 表示範囲外にコピーしようとした場合、コピー先の日を SelectedDate に設定する
            if (needsChangeSelectedDate)
            {
                var removed = new List<MyAppointment>();
                // コピー後の使い勝手を良くするため、特定の表示形式の場合は、コピー処理に伴ってそれを変更する
                if (parent.PeriodType.Value == InputPeriodType.OneDay)
                {
                    // 表示形式が「日」だった場合、コピー後に「過去3日」に変更する
                    parent.SetValueWithoutLoading(() => parent.PeriodType.Value = InputPeriodType.Last3Days);

                    // 新しい表示範囲に含まれる編集中の予定は退避させておき、更新の処理が走った後に追加する
                    // これにより形式変更後も表示範囲に含まれる予定に関しては、削除の確認ダイアログが出ないようにしている
                    var editedApos = parent.Appointments.Where(a => a.IsMyWork.Value && a.ApoType == AppointmentType.Manual).ToList();
                    removed = editedApos.Where(a => InputPeriodType.Last3Days.Contains(toDate, a.Start.Date, parent.Parent.Settings.Calendar)).ToList();
                    foreach (var a in removed)
                    {
                        parent.Appointments.Remove(a);
                        targets.Add(a);
                    }
                }
                else if (parent.PeriodType.Value == InputPeriodType.WorkingDays)
                {
                    // 表示形式が「稼働日」で、コピー先の日が休みの日だった場合（金曜→土曜のようなコピーを想定）
                    if (isToDateHoliday)
                    {
                        // 表示形式をコピー後に「週」に変更する（コピー先の日も表示されるようにする）
                        parent.SetValueWithoutLoading(() => parent.PeriodType.Value = InputPeriodType.ThisWeek);

                        var editedApos = parent.Appointments.Where(a => a.IsMyWork.Value && a.ApoType == AppointmentType.Manual).ToList();
                        removed = editedApos.Where(a => InputPeriodType.ThisWeek.Contains(toDate, a.Start.Date, parent.Parent.Settings.Calendar)).ToList();
                        foreach (var a in removed)
                        {
                            parent.Appointments.Remove(a);
                            targets.Add(a);
                        }
                    }
                }

                parent.SelectedDate.Value = toDate;

                // 編集済みの予定があった場合、保存するかどうかを確認し、キャンセルされた場合、SelectedDate は更新されない。
                // この場合、本機能も何もせずに終了する。
                if (parent.SelectedDate.Value != toDate)
                {
                    if (removed.Any())
                        parent.Appointments.AddRange(removed);
                    return;
                }
            }

            parent.Appointments.AddRange(targets);
        }
    }
}
