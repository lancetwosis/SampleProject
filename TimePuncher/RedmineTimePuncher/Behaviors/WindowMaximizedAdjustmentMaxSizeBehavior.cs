using LibRedminePower.Behaviors.Bases;
using Reactive.Bindings.ObjectExtensions;
using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Behaviors
{
    /// <summary>
    /// TimePuncherでは"RadRibbonWindowStyle"にて「WindowsStyle="None"」に設定しているため、最大化時に画面下部がタスクバーの下に隠れてしまう。
    /// このBehaviorでは、最大化時にタスクバーを考慮したMaxHeight、MaxWidthを決定する。
    /// ただし、現状「MaxHeight、MaxWidth」に+5をしないと余白が出来てしまう。
    /// </summary>
    public class WindowMaximizedAdjustmentMaxSizeBehavior : BehaviorBase<Window>
    {
        protected override void OnSetup()
        { 
            base.OnSetup();
            AssociatedObject.Loaded += onWindowStateChanged;
            AssociatedObject.StateChanged += onWindowStateChanged;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            AssociatedObject.Loaded -= onWindowStateChanged;
            AssociatedObject.StateChanged -= onWindowStateChanged;
        }

        private void onWindowStateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window.WindowState == WindowState.Maximized)
            {
                var currentScreen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle);
                var windowBorderThickness = SystemParameters.WindowResizeBorderThickness;

                window.MaxHeight = currentScreen.WorkingArea.Height + windowBorderThickness.Left + windowBorderThickness.Right + 5;
                window.MaxWidth = currentScreen.WorkingArea.Width + windowBorderThickness.Top + windowBorderThickness.Bottom + 5;
            }
            else
            {
                window.MaxHeight = double.PositiveInfinity;
                window.MaxWidth = double.PositiveInfinity;
            }
        }
    }
}