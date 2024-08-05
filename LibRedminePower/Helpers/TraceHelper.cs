using LibRedminePower.Extentions;
using LibRedminePower.Localization;
using LibRedminePower.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace LibRedminePower.Helpers
{
    public static class TraceHelper
    {
        public static void Init()
        {
            TraceMonitor.AnalyticsMonitor = new GoogleAnalytics.CustomAnalitycsMonitor("UA-201230008-1", "G-D9BSJFGQS1");
        }

        public static void TrackAtomicFeature(string feature,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Logger.Info(feature, callerMemberName, callerFilePath, callerLineNumber);
            TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(feature);
        }

        public static void TrackCommand(string command,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            TrackAtomicFeature($"{command}.Executed", callerMemberName, callerFilePath, callerLineNumber);
        }

    }
}
