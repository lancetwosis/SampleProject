using LibRedminePower.Extentions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.ViewModels.Input.Resources;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using RedmineTimePuncher.ViewModels.Settings;
using RedmineTimePuncher.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls.ScheduleView;
using Telerik.Windows.DragDrop;
using Telerik.Windows.DragDrop.Behaviors;

namespace RedmineTimePuncher.Behaviors
{
    public class MyScheduleViewDragDropBehavior : ScheduleViewDragDropBehavior
    {
        public MyScheduleViewDragDropBehavior()
        {
            CacheConvertedDragData = true;
        }

        public override IEnumerable<IOccurrence> ConvertDraggedData(object data)
        {
            InputView.IsUrlDragging = false;

            // チケット一覧からのドラッグ
            var draggedItem = DragDropPayloadManager.GetDataFromObject(data, "DraggedData") as MyIssue;
            if (draggedItem != null)
            {
                return new List<DraggedAppointment>() { new DraggedAppointment(draggedItem) };
            }
            // 作業分類一覧からのドラッグ
            else if (DataObjectHelper.GetDataPresent(data, typeof(MyCategory), false))
            {
                var category = ((IEnumerable)DataObjectHelper.GetData(data, typeof(MyCategory), true)).Cast<MyCategory>().First();
                if (category != null)
                {
                    var appointment = new DraggedAppointment(DraggedAppointment.DraggedType.MyCategory);
                    appointment.End = appointment.Start;
                    appointment.Category = category;
                    return new List<DraggedAppointment>() { appointment };
                }
            }
            // ブラウザからのドラッグ
            else if (DataObjectHelper.GetDataPresent(data, "UniformResourceLocator", false))
            {
                var url = DataObjectHelper.GetData(data, typeof(string), true) as string;
                var ticketNo = RedmineManager.Default.Value.GetTicketNoFromUrl(url);
                if (!string.IsNullOrEmpty(ticketNo))
                {
                    var ticket = RedmineManager.Default.Value.GetTicketIncludeJournal(ticketNo, out var _);
                    if (ticket != null)
                    {
                        InputView.IsUrlDragging = true;
                        return new[] { new DraggedAppointment(ticket) };
                    }
                }
            }

            return null;
        }

        public override bool CanDrop(Telerik.Windows.Controls.DragDropState state)
        {
            if (state.Appointment is DraggedAppointment apo)
            {
                switch (apo.From)
                {
                    // チケット一覧、もしくはブラウザからのドラッグ
                    case DraggedAppointment.DraggedType.Ticket:
                        {
                            // MyWorks 以外には、ドロップすることができない
                            if (!state.DestinationSlots.First().Resources.Cast<MyResourceBase>().Any(a => a.IsMyWorks()))
                                return false;

                            // 有効でないプロジェクトのチケットはドロップできない
                            if (apo.Ticket != null && apo.ProjectStatus == ProjectStatusType.NotActive)
                                return false;

                            return base.CanDrop(state);
                        }
                    // 作業分類一覧からのドラッグ
                    case DraggedAppointment.DraggedType.MyCategory:
                        {
                            // MyWorks または Members 以外には、ドロップすることができない
                            var rs = state.DestinationSlots.First().Resources.Cast<MyResourceBase>().ToList();
                            if (!rs.Any(a => a.IsMyWorks()) && !rs.Any(a => a.IsMembers()))
                                return false;

                            var targetedAppointment = state.TargetedAppointment as MyAppointment;
                            if (targetedAppointment == null || targetedAppointment.Category == apo.Category)
                                return false;

                            // 有効でないプロジェクトの作業実績へはドロップできない
                            if (targetedAppointment.Ticket != null && targetedAppointment.ProjectStatus == ProjectStatusType.NotActive)
                                return false;

                            if (targetedAppointment.ProjectCategories.Value == null ||
                                !targetedAppointment.ProjectCategories.Value.Any(c => c.DisplayName == apo.Category.DisplayName))
                            {
                                return false;
                            }
                            else
                            {
                                if (targetedAppointment.IsMyWork.Value || targetedAppointment.ApoType == AppointmentType.RedmineTimeEntryMember)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    default:
                        throw new NotSupportedException($"apo.From が {apo.From} はサポート対象外です。");
                }
            }
            // ScheduleView 内でのドラッグ
            else if (state.Appointment is MyAppointment myAppo)
            {
                var destSlot = state.DestinationSlots.First();

                // MyWorks 以外には、ドロップすることができない
                if (!destSlot.Resources.Cast<MyResourceBase>().Any(a => a.IsMyWorks()))
                    return false;

                // 有効でないプロジェクトのチケットに紐づいた予定はドロップできない
                if (myAppo.Ticket != null && myAppo.ProjectStatus == ProjectStatusType.NotActive)
                    return false;

                // 有効でないプロジェクトの作業実績へはドロップできない
                var target = state.TargetedAppointment as MyAppointment;
                if (target != null && target.Ticket != null && target.ProjectStatus == ProjectStatusType.NotActive)
                    return false;

                // MyWorks 以外からドラッグ or 日を跨いでのドラッグの場合は、強制コピーモードとする。
                if ((state.SourceResources != null && !state.SourceResources.Cast<MyResourceBase>().Any(a => a.IsMyWorks())) ||
                    myAppo.Start.GetDateOnly() != destSlot.Start.GetDateOnly())
                {
                    state.IsControlPressed = true;
                }

                return true;
            }

            return false;
        }

        public override void Drop(Telerik.Windows.Controls.DragDropState state)
        {
            var scheduleView = state.ServiceProvider.GetService<IDialogProvider>() as RadScheduleView;

            if (state.Appointment is DraggedAppointment apo)
            {
                if (state.TargetedAppointment is MyAppointment target)
                {
                    switch (apo.From)
                    {
                        case DraggedAppointment.DraggedType.Ticket:
                            if (!state.IsControlPressed)
                            {
                                scheduleView.BeginEdit(target);
                                target.TicketNo = apo.TicketNo;
                                scheduleView.Commit();
                            }
                            else
                            {
                                // Controlを押下していたら、追加する。
                                base.Drop(state);
                            }
                            return;
                        case DraggedAppointment.DraggedType.MyCategory:
                            scheduleView.BeginEdit(target);
                            // 自身に有効な作業分類から「名称」が一致するものを設定する
                            target.Category = target.ProjectCategories.Value.First(c => c.DisplayName == apo.Category.DisplayName);
                            scheduleView.Commit();

                            if(target.ApoType == Enums.AppointmentType.RedmineTimeEntryMember)
                            {
                                // 親チケットの編集フラグを立てる。
                                var apos = scheduleView.AppointmentsSource as ObservableCollection<MyAppointment>;
                                var parent = apos.FirstOrDefault(a => a.MemberAppointments.Contains(target));
                                if(parent != null)
                                {
                                    scheduleView.BeginEdit(parent);
                                    parent.ApoType = Enums.AppointmentType.Manual;
                                    scheduleView.Commit();
                                }
                            }

                            return;
                        default:
                            throw new NotSupportedException($"apo.From が {apo.From} はサポート対象外です。");
                    }
                }
                else
                {
                    switch (apo.From)
                    {
                        case DraggedAppointment.DraggedType.Ticket:
                            base.Drop(state);
                            return;
                        case DraggedAppointment.DraggedType.MyCategory:
                            throw new NotSupportedException($"CanDropを制御しているので、ここに来ることはない。");
                        default:
                            throw new NotSupportedException($"apo.From が {apo.From} はサポート対象外です。");
                    }
                }
            }
            else if (state.Appointment is MyAppointment myApo)
            {
                scheduleView.AppointmentCreated += ScheduleView_AppointmentCreated;
                base.Drop(state);
                scheduleView.AppointmentCreated -= ScheduleView_AppointmentCreated;
            }
            base.Drop(state);
        }

        public override void DragDropCompleted(Telerik.Windows.Controls.DragDropState state)
        {
            // ScheduleView の外(チケット一覧の GridView など)にドロップしたら強制コピーモード
            if (state.DestinationAppointmentsSource == null)
                state.IsControlPressed = true;

            base.DragDropCompleted(state);
        }

        private void ScheduleView_AppointmentCreated(object sender, AppointmentCreatedEventArgs e)
        {
            if (e.CreatedAppointment != null && e.CreatedAppointment is MyAppointment apo)
            {
                apo.MemberAppointments = new List<MyAppointment>();
            }
        }
    }
}