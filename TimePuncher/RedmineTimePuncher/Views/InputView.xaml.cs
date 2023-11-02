using LibRedminePower.Behaviors.Bases;
using Microsoft.Win32;
using RedmineTimePuncher.Behaviors;
using RedmineTimePuncher.ViewModels;
using RedmineTimePuncher.ViewModels.Input;
using RedmineTimePuncher.ViewModels.Input.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

            DataContextChanged += InputView_DataContextChanged;
        }

        private void InputView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null)
                return;

            var vm = (DataContext as MainWindowViewModel).Input;
            vm.SelectedDate.Subscribe(d =>
            {
                this.calendar.DisplayDate = d;
                applyDayTemplate();
            });
            vm.DisplayStartTime.CombineLatest(vm.DisplayEndTime, (_, __) => true).Subscribe(_ =>
            {
                applyDayTemplate();
            });
        }

        private void applyDayTemplate()
        {
            // DayTemplate を適用するために DisplayDate を切り替えて SelectTemplate を実行させる
            this.calendar.DisplayDate = this.calendar.DisplayDate.AddMonths(1);
            this.calendar.DisplayDate = this.calendar.DisplayDate.AddMonths(-1);
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

        // NavigationView 対応の影響？で FirstVisibleTime での設定ができなくなったため
        // Loaded のタイミングで手動でスクロールする
        // https://docs.telerik.com/devtools/wpf/controls/radscheduleview/features/timeruler/scrolling
        private void scheduleView_Loaded(object sender, RoutedEventArgs e)
        {
            scrollToWorkStartTime(sender as RadScheduleView);
        }

        private void scheduleView_VisibleRangeChanged(object sender, EventArgs e)
        {
            scrollToWorkStartTime(sender as RadScheduleView);
        }

        private void scrollToWorkStartTime(RadScheduleView schedule)
        {
            var vm = schedule.DataContext as MainWindowViewModel;
            schedule.ScrollTimeRuler(vm.Settings.Schedule.WorkStartTime, true);
        }
    }
}
