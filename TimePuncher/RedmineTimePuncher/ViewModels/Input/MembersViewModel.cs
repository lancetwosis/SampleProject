using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input
{
    public class MembersViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReadOnlyReactivePropertySlim<List<MemberResource>> Resources { get; }

        public CommandBase AddMemberCommand { get; set; }
        public CommandBase RemoveMemberCommand { get; set; }
        public CommandBase AgreeMemberWork { get; }

        public MembersViewModel(InputViewModel parent)
        {
            // ユーザー情報を元にリソースを作成する。
            Resources = parent.Parent.Settings.ObserveProperty(a => a.User).Select(u => u.Items.Select(a => new MemberResource(a, parent.MyWorks.Resource)).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Resources.SubscribeWithErr(users =>
            {
                var members = parent.MyType.Resources.OfType<MemberResource>().ToList();
                foreach (var memberResource in members)
                {
                    memberResource.Dispose();
                    parent.MyType.Resources.Remove(memberResource);
                }
                if (users != null) parent.MyType.Resources.AddRange(users);
                parent.ResourceTypes.Remove(parent.MyType);
                parent.ResourceTypes.Add(parent.MyType);
            }).AddTo(disposables);
            Resources.CombineLatest(parent.Parent.Redmine.Where(a => a != null),
                (users, r) => users.Select(a => a.User).Concat(new[] { r.MyUser }).ToList())
                .SubscribeWithErr(a => MyTimeEntry.DicUsers = a.ToDictionary(b => b.Id)).AddTo(disposables);

            var isAllMyWork = parent.SelectedAppointments.Select(a => a != null && a.Any() && a.All(b => b.IsMyWork.Value));
            var isAllMyWork_AllMember = new[]
            {
                isAllMyWork,
                parent.SelectedAppointments.Select(a => a != null && a.All(b => b.ApoType == AppointmentType.RedmineTimeEntryMember)),
            }.CombineLatestValuesAreAllTrue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var isAllMyWork_AllNotMember = new[]
            {
                isAllMyWork,
                parent.SelectedAppointments.Select(a => a != null && a.All(b => b.ApoType != AppointmentType.RedmineTimeEntryMember)),
            }.CombineLatestValuesAreAllTrue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // メンバーリソースから共同作業追加コマンドを作成する。
            var canAddMember = new[] {
                isAllMyWork_AllNotMember,
                Resources.Select(a => a.Any()),
            }.CombineLatestValuesAreAllTrue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var addCmds = Resources.Select(resList => resList.Select(res =>
            {
                return new ChildCommand(res.DisplayName, canAddMember.Select(a => a ? null : ""), () =>
                    {
                        TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(AddMemberCommand) + ".Executed");

                        parent.SelectedAppointments.Value.ForEach(apo =>
                        {
                            if (!apo.MemberAppointments.Any(a => a.Resources.First() == res))
                            {
                                apo.ApoType = AppointmentType.Manual;

                                var newApo = apo.Copy() as MyAppointment;
                                apo.MemberAppointments.Add(newApo);
                                newApo.ApoType = AppointmentType.RedmineTimeEntryMember;
                                newApo.Resources.Clear();
                                newApo.Resources.Add(res);
                                parent.Appointments.Add(newApo);

                                // 画面を更新するためのおまじない
                                parent.Appointments.Remove(apo);
                                parent.Appointments.Add(apo);
                            }
                        });

                        // 選択予定をリセットすることで、コマンドの実行可否を更新する。
                        var temp = parent.SelectedAppointments.Value;
                        parent.SelectedAppointments.Value = null;
                        parent.SelectedAppointments.Value = temp;
                    });
            }).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            AddMemberCommand = new CommandBase(
                            Properties.Resources.RibbonCmdCoopRegister, Properties.Resources.icons8_add_user_group_man_man_48,
                            canAddMember.Select(a => a ? null : ""),
                            addCmds).AddTo(disposables);
            AddMemberCommand.MenuText = Properties.Resources.ScheduleViewCmdCoopRegister;

            // メンバーリソースから共同作業解除コマンドを作成する。
            var canRemoveMember = new[]
            {
                canAddMember,
                parent.SelectedAppointments.Select(a => a != null && a.All(b => b.MemberAppointments.Any())),
            }.CombineLatestValuesAreAllTrue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var removeCmds = Resources.Select(resList => resList.Select(res =>
            {
                return new ChildCommand(res.DisplayName, canRemoveMember.Select(a => a ? null : ""), () =>
                                        {
                                            TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(RemoveMemberCommand) + ".Executed");

                                            parent.SelectedAppointments.Value.ForEach(apo =>
                                            {
                                                var memberApo = apo.MemberAppointments.FirstOrDefault(a => a.Resources.First()?.ResourceName == res.ResourceName);
                                                if (memberApo != null)
                                                {
                                                    apo.ApoType = AppointmentType.Manual;
                                                    apo.MemberAppointments.Remove(memberApo);
                                                    parent.Appointments.Remove(memberApo);

                                                    // 画面を更新するためのおまじない
                                                    parent.Appointments.Remove(apo);
                                                    parent.Appointments.Add(apo);
                                                }
                                            });

                                            // 選択予定をリセットすることで、コマンドの実行可否を更新する。
                                            var temp = parent.SelectedAppointments.Value;
                                            parent.SelectedAppointments.Value = null;
                                            parent.SelectedAppointments.Value = temp;
                                        });
            }).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            RemoveMemberCommand = new CommandBase(
                Properties.Resources.RibbonCmdCoopRemove, Properties.Resources.icons8_banned_remove_48,
                canRemoveMember.Select(a => a ? null : ""),
                removeCmds).AddTo(disposables);
            RemoveMemberCommand.MenuText = Properties.Resources.ScheduleViewCmdCoopRemove;

            // 共同作業を承認する。
            AgreeMemberWork = new CommandBase(
                Properties.Resources.RibbonCmdApprove, Properties.Resources.icons8_checked_radio_button_48,
                new[] {
                    parent.SelectedAppointments.Select(a => a == null || !a.Any()).Select(a => a ? Properties.Resources.msgErrSelectAppointments : null),
                    parent.SelectedAppointments.Select(a => a != null && a.Any(b => !b.IsMyWork.Value)).Select(a => a ? Properties.Resources.msgErrSelectAppoOtherThanSpentTime : null),
                    parent.SelectedAppointments.Select(a => a != null && a.Any(b => b.ApoType != AppointmentType.RedmineTimeEntryMember)).Select(a  => a ? Properties.Resources.RibbonCmdApproveMsgSelectCollaboration : null),
                }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(AgreeMemberWork) + ".Executed");

                    foreach (var item in parent.SelectedAppointments.Value)
                    {
                        item.ApoType = AppointmentType.Manual;
                        item.FromEntryId = item.TimeEntryId;
                        item.TimeEntryId = -1;

                        // 画面を更新するためのおまじない
                        parent.Appointments.Remove(item);
                        parent.Appointments.Add(item);
                    }

                    // 選択予定をリセットすることで、コマンドの実行可否を更新する。
                    var temp = parent.SelectedAppointments.Value;
                    parent.SelectedAppointments.Value = null;
                    parent.SelectedAppointments.Value = temp;
                }).AddTo(disposables);
            AgreeMemberWork.MenuText = Properties.Resources.ScheduleViewCmdCoopApprove;
        }
    }
}
