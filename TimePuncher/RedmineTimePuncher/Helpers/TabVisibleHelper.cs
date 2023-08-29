using RedmineTimePuncher.Enums;
using RedmineTimePuncher.ViewModels.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Helpers
{
    public class TabVisibleHelper
    {
        public static DependencyProperty EnableModeProperty = DependencyProperty.RegisterAttached(
            "EnbaleMode",
            typeof(string),
            typeof(TabVisibleHelper),
            new PropertyMetadata(null));

        public static string GetEnbaleMode(DependencyObject obj)
        {
            return (string)obj.GetValue(EnableModeProperty);
        }

        public static void SetEnbaleMode(DependencyObject obj, string value)
        {
            obj.SetValue(EnableModeProperty, value);
        }

        public static DependencyProperty EnableModesProperty = DependencyProperty.RegisterAttached(
            "EnableModes",
            typeof(string),
            typeof(TabVisibleHelper),
            new PropertyMetadata(null));

        public static string GetEnableModes(DependencyObject obj)
        {
            return (string)obj.GetValue(EnableModesProperty);
        }

        public static void SetEnableModes(DependencyObject obj, string value)
        {
            obj.SetValue(EnableModesProperty, value);
        }


        public static DependencyProperty CurrentModeProperty = DependencyProperty.RegisterAttached(
            "CurrentMode",
            typeof(ApplicationMode),
            typeof(TabVisibleHelper),
            new PropertyMetadata(ApplicationMode.Unselected, currentModeChanged));

        public static ApplicationMode GetCurrentMode(DependencyObject obj)
        {
            return (ApplicationMode)obj.GetValue(CurrentModeProperty);
        }

        public static void SetCurrentMode(DependencyObject obj, ApplicationMode value)
        {
            obj.SetValue(CurrentModeProperty, value);
        }


        private static void currentModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var currentMode = (ApplicationMode)e.NewValue;
            if (currentMode == ApplicationMode.Unselected)
                return;

            var ribbonTab = d as RadRibbonTab;
            if (ribbonTab != null)
            {
                if (currentMode == FastEnumUtility.FastEnum.Parse<ApplicationMode>(GetEnbaleMode(d)))
                {
                    ribbonTab.IsSelected = true;
                    ribbonTab.Visibility = Visibility.Visible;
                }
                else
                {
                    ribbonTab.IsSelected = false;
                    ribbonTab.Visibility = Visibility.Collapsed;
                }
                return;
            }

            var tab = d as RadTabItem;
            if (tab != null)
            {
                if (GetEnableModes(d).Split(',').Select(s => FastEnumUtility.FastEnum.Parse<ApplicationMode>(s)).Contains(currentMode))
                {
                    // 設定ダイアログの場合、常に Redmine のタブを初期選択させたいため IsSelected は設定しない
                    // 現状、設定ダイアログを開いたまま ApplicationMode を切り替える処理はないため、この処理とする。必要が出たら別途検討すること。
                    tab.Visibility = Visibility.Visible;
                }
                else
                {
                    tab.IsSelected = false;
                    tab.Visibility = Visibility.Collapsed;
                }
                return;
            }
        }
    }
}
