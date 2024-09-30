using LibRedminePower.Applications;
using LibRedminePower.Exceptions;
using LibRedminePower.Helpers;
using LibRedminePower.Interfaces;
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
    public static class ErrorHandler
    {
        private static IErrorHandler _instance;
        public static IErrorHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefaultErrorHandler();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }

    public class DefaultErrorHandler : IErrorHandler
    {
        public void HandleError(Exception e)
        {
            var needsContinue = handleError(e);
            if (!needsContinue)
                Environment.Exit(-1);
        }

        private bool handleError(Exception e, bool showMessage = true)
        {
            // MainWindowViewModelのコンストラクタで例外が発生すると、スプラッシュが残っている場合があるので、一旦クローズする。
            RadSplashScreenManager.Close();

            Logger.Error(e, "Show exception message.");

            // 正常例外
            if (e is OperationCanceledException)
                return true;
            else if (e is ApplicationExitException)
            {
                if (showMessage)
                    MessageBoxHelper.ConfirmError(e.Message);
                return false;
            }
            else if (e is ApplicationContinueException)
            {
                return handleError(e.InnerException, false);
            }
            else if (e is ApplicationException ||
                e is IOException ||
                e is Redmine.Net.Api.Exceptions.RedmineException ||
                e is UnauthorizedAccessException)
            {
                if (showMessage)
                    MessageBoxHelper.ConfirmError(e.Message);
                return true;
            }
            else if (e is AggregateException ae)
            {
                var result = true;
                foreach (var ie in ae.InnerExceptions)
                {
                    var r = handleError(ie, showMessage);
                    if (!r)
                        result = false;
                }
                return result;
            }
            // 異常例外
            else if (e is XamlParseException xpe)
            {
                HandleError(xpe.InnerException);
                return false;
            }
            else
            {
                if (!showMessage) return true;

#if DEBUG
                var r = MessageBoxHelper.ConfirmError(string.Format(Properties.Resources.ExceptionMsgConfirm, e.ToString()), MessageBoxHelper.ButtonType.OkCancel);
                return r != null ? r.Value : true;
#else
                if (TraceMonitor.AnalyticsMonitor != null)
                    TraceMonitor.AnalyticsMonitor.TrackError("Exception", e);

                MessageBoxHelper.ConfirmError(string.Format(Properties.Resources.ExceptionMsgExit, ApplicationInfo.Title));
                return false;
#endif
            }
        }
    }
}
