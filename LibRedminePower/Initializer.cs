using GoogleAnalytics;
using LibRedminePower.Applications;
using LibRedminePower.Exceptions;
using LibRedminePower.Helpers;
using LibRedminePower.Localization;
using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Schedulers;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Telerik.Windows.Automation.Peers;
using Telerik.Windows.Controls;
using Telerik.Windows.Input.Touch;

namespace LibRedminePower
{
    public static class Initializer
    {
        public static void Init(Application app)
        {
            // ログ設定
            Logger.Init();
            Logger.Info($"{ApplicationInfo.Title} has started.");

            // これを入れておかないと、Redmine4.2にて、RedmineAPI発行時にエラーになる場合がある。
            ServicePointManager.Expect100Continue = false;

            // ReactivePropertyの初期化
            ReactivePropertyScheduler.SetDefault(new ReactivePropertyWpfScheduler(app.Dispatcher));

            // 高速化
            AutomationManager.AutomationMode = AutomationMode.Disabled;
            TouchManager.IsTouchEnabled = false;

            // 集約例外設定
            app.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            // 前バージョンからのUpgradeを実行していないときは、Upgradeを実施する
            if (Properties.Settings.Default.IsUpgrade == false)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgrade = true;
                Properties.Settings.Default.Save();
            }
        }
        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ErrorHandler.Instance.HandleError(e.Exception);
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorHandler.Instance.HandleError((Exception)e.ExceptionObject);
        }
    }
}
