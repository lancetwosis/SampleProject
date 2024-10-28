using LibRedminePower.Behaviors;
using LibRedminePower.Behaviors.Bases;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.ViewModels.Input.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls.ScheduleView;
using Telerik.Windows.DragDrop;
using Telerik.Windows.DragDrop.Behaviors;

namespace RedmineTimePuncher.Behaviors
{
    public class MyGridViewDragDropBehavior : BehaviorBase<RadGridView>
    {
        protected override void OnSetup()
        {
            this.unsubscribeFromDragDropEvents();
            this.subscribeToDragDropEvents();
        }

        protected override void OnCleanup()
        {
            this.unsubscribeFromDragDropEvents();
        }

        private void subscribeToDragDropEvents()
        {
            DragDropManager.SetAllowDrag(this.AssociatedObject, true);
            DragDropManager.AddDragInitializeHandler(this.AssociatedObject, OnDragInitialize);
            DragDropManager.AddDragDropCompletedHandler(this.AssociatedObject, OnDragDropCompleted);
            DragDropManager.AddDropHandler(this.AssociatedObject, OnDrop);
            DragDropManager.AddDragOverHandler(this.AssociatedObject, OnDragOver);
            
        }

        private void unsubscribeFromDragDropEvents()
        {
            DragDropManager.RemoveDragInitializeHandler(this.AssociatedObject, OnDragInitialize);
            DragDropManager.RemoveDragDropCompletedHandler(this.AssociatedObject, OnDragDropCompleted);
            DragDropManager.RemoveDropHandler(this.AssociatedObject, OnDrop);
            DragDropManager.RemoveDragOverHandler(this.AssociatedObject, OnDragOver);
        }

        private void OnDrop(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            // from a web browser
            if (DataObjectHelper.GetDataPresent(e.Data, "UniformResourceLocator", false))
            {
                var url = DataObjectHelper.GetData(e.Data, typeof(string), true) as string;
                var ticketNo = RedmineManager.Default.Value.GetTicketNoFromUrl(url);
                if (!string.IsNullOrEmpty(ticketNo))
                {
                    var ticket = RedmineManager.Default.Value.GetTicketIncludeJournal(ticketNo, out var _);
                    if (ticket != null)
                    {
                        var vm = AssociatedObject.DataContext as TicketGridViewModel;
                        vm.AddFavoriteTicket(ticket);
                    }
                }
            }
            // from ScheduleView
            else if (DataObjectHelper.GetDataPresent(e.Data, typeof(ScheduleViewDragDropPayload), false))
            {
                var appos = DataObjectHelper.GetData(e.Data, typeof(ScheduleViewDragDropPayload), true) as ScheduleViewDragDropPayload;
                if (appos != null)
                {
                    var vm = AssociatedObject.DataContext as TicketGridViewModel;
                    foreach (var a in appos.DraggedAppointments.OfType<MyAppointment>().Where(a => a.Ticket != null))
                    {
                        vm.AddFavoriteTicket(a.Ticket);
                    }
                }
            }

            e.Handled = true;
        }

        private void OnDragInitialize(object sender, DragInitializeEventArgs e)
        {
            var row = e.OriginalSource as GridViewRow ?? (e.OriginalSource as FrameworkElement).ParentOfType<GridViewRow>();

            var grid = sender as RadGridView;
            var item = row != null ? row.Item : grid.SelectedItem;

            var dragPayload = DragDropPayloadManager.GeneratePayload(null);
            dragPayload.SetData("DraggedData", item);

            e.Data = dragPayload;
            e.DragVisualOffset = e.RelativeStartPoint;
            e.AllowedEffects = DragDropEffects.All;

            var vm = grid.DataContext as TicketGridViewModel;
            vm.IsDragDropRunning = true;
        }

        private void OnDragDropCompleted(object sender, DragDropCompletedEventArgs e)
        {
            var vm = (sender as RadGridView).DataContext as TicketGridViewModel;
            vm.IsDragDropRunning = false;
        }

        private void OnDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            var vm = (sender as RadGridView).DataContext as TicketGridViewModel;

            // お気に入りタブじゃなかった場合、ドラッグさせない
            if (vm.Title.Value != Properties.Resources.IssueGridFavorites)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // from a web browser
            if (DataObjectHelper.GetDataPresent(e.Data, "UniformResourceLocator", false))
            {
                var url = DataObjectHelper.GetData(e.Data, typeof(string), true) as string;
                var ticketNo = RedmineManager.Default.Value.GetTicketNoFromUrl(url);
                if (string.IsNullOrEmpty(ticketNo))
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            // from ScheduleView
            else if (DataObjectHelper.GetDataPresent(e.Data, typeof(ScheduleViewDragDropPayload), false))
            {
                // 追加の条件なし
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }
    }
}
