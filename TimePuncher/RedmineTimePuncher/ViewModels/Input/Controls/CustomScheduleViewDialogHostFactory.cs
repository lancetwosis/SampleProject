using LibRedminePower.Applications;
using System;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.ViewModels.Input.Controls
{
    public class CustomScheduleViewDialogHostFactory : ScheduleViewDialogHostFactory
    {
        protected override IScheduleViewDialogHost CreateNew(ScheduleViewBase scheduleView, DialogType dialogType)
        {
            var host = base.CreateNew(scheduleView, dialogType);
            var window = host as RadWindow;

            if (dialogType == DialogType.ConfirmationDialog)
            {
                window.Activated += Window_Activated;
            }

            return host;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            SchedulerWindow schedulerWindow = (SchedulerWindow)sender;
            var scheduleView = schedulerWindow.ScheduleView;
            var scheduleViewWindow = scheduleView.ParentOfType<Window>();

            schedulerWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            schedulerWindow.Left = scheduleViewWindow.Left + (scheduleViewWindow.Width - schedulerWindow.ActualWidth) / 2;
            schedulerWindow.Top = scheduleViewWindow.Top + (scheduleViewWindow.Height - schedulerWindow.ActualHeight) / 2;
            schedulerWindow.Activated -= Window_Activated;
        }
    }
}
