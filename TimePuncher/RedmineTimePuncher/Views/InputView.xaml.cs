using LibRedminePower.Behaviors.Bases;
using Microsoft.Win32;
using RedmineTimePuncher.Behaviors;
using RedmineTimePuncher.ViewModels;
using RedmineTimePuncher.ViewModels.Input;
using RedmineTimePuncher.ViewModels.Input.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls.ScheduleView;
using Telerik.Windows.DragDrop;
using Telerik.Windows.DragDrop.Behaviors;

namespace RedmineTimePuncher.Views
{
    /// <summary>
    /// InputView.xaml の相互作用ロジック
    /// </summary>
    public partial class InputView : UserControl
    {
        public static bool IsUrlDragging = false;

        private Style dragDropHighlightStyleCache;
        private Style dragDropHighlightStyle;

        public InputView()
        {
            InitializeComponent();

            if (Properties.Settings.Default.MinTimeRulerExtent > 0)
                this.scheduleView.ActiveViewDefinition.MinTimeRulerExtent = Properties.Settings.Default.MinTimeRulerExtent;

            DragDropManager.AddDragOverHandler(this.scheduleView, OnScheduleViewDragOver, true);
            dragDropHighlightStyleCache = this.scheduleView.DragDropHighlightStyle;
            dragDropHighlightStyle = (Style)this.Resources["AppointmentDragDropHighlightStyle"];
        }


        private void OnScheduleViewDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            // note that if the IsContentPreserved property of RadTabControl is set to True, the ChildrenOfType<T> method may return multiple RadGridView instances.
            // in this case you will need to find the one associated with the selected tab
            var gridView = this.tabControl.ChildrenOfType<RadGridView>().FirstOrDefault();
            var vm = gridView.DataContext as TicketGridViewModel;
            if (vm.IsDragDropRunning || IsUrlDragging)
            {
                var slot = scheduleView.HighlightedSlots.LastOrDefault(x => x is DragDropSlot);
                if (slot != null)
                {
                    var dropTargetAppointment = this.scheduleView.AppointmentsSource.OfType<Appointment>().FirstOrDefault(x => x.Resources.Any(a => slot.Resources.Contains(a)) && (x.Start <= slot.Start && slot.Start < x.End));
                    if (dropTargetAppointment != null)
                        scheduleView.DragDropHighlightStyle = dragDropHighlightStyle;
                    else
                        scheduleView.DragDropHighlightStyle = dragDropHighlightStyleCache;
                }
            }
        }

        // 編集ダイアログが開かないようにする
        // https://docs.telerik.com/devtools/wpf/controls/radscheduleview/howto/prevent-dialogs-from-opening
        private void scheduleView_ShowDialog(object sender, ShowDialogEventArgs e)
        {
            if (e.DialogViewModel is AppointmentDialogViewModel)
                e.Cancel = true;
        }

        // #278 ScheduleView の予定のツールチップが意図せず他のアプリケーションの前面に表示されてしまう。 の対応
        private void OnAppointmentToolTipOpening(object sender, ToolTipEventArgs e)
        {
            AppointmentItem appointmentItem = e.Source as AppointmentItem;
            if (appointmentItem != null && !appointmentItem.IsMouseOver)
            {
                // isMouseoverでなければ、実行済みとしToolTipを表示しない。
                e.Handled = true;
            }
        }

        private void scheduleView_Loaded(object sender, RoutedEventArgs e)
        {
            // NavigationView 対応の影響？で FirstVisibleTime での設定がなぜかできなくなったためこちらで代替する
            var schedule = sender as RadScheduleView;
            var vm = schedule.DataContext as MainWindowViewModel;
            schedule.ScrollTimeRuler(vm.Settings.Schedule.WorkStartTime, true);
        }
    }
}
