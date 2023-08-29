using GoogleAnalytics;
using LibRedminePower.Applications;
using LibRedminePower.Logging;
using System;
using System.Linq;
using System.Management;
using Telerik.Windows.Controls;

namespace LibRedminePower.GoogleAnalytics
{
    public class CustomAnalitycsMonitor : ITraceMonitor
    {
        private TrackerManager trackerManager;
        private Tracker tracker;
        private Tracker trackerGA4;

        public CustomAnalitycsMonitor(string propertyId, string propertyIdGA4)
        {
            trackerManager = new TrackerManager(new PlatformInfoProvider()
            {
                AnonymousClientId = getAnonymousClientId(),
                UserAgent = getUserAgent(),
                UserLanguage = System.Globalization.CultureInfo.CurrentCulture.Name,
                ScreenResolution = new Dimensions(1920, 1080),      // それほど重要ではないので、固定で使う。
                ViewPortResolution = new Dimensions(1920, 1080)     // それほど重要ではないので、固定で使う。
            });

            tracker = trackerManager.CreateTracker(propertyId); // your GoogleAnalytics property tracking ID goes here 
            tracker.AppName = ApplicationInfo.Title;
            tracker.AppVersion = ApplicationInfo.Version.ToString();

            trackerGA4 = trackerManager.CreateTracker(propertyIdGA4); // your GoogleAnalytics property measurement ID goes here 
            trackerGA4.AppName = ApplicationInfo.Title;
            trackerGA4.AppVersion = ApplicationInfo.Version.ToString();
        }

        private string getAnonymousClientId()
        {
#if DEBUG
            // デバック版では固定する。
            return "b597d28a-0d6c-42ed-9dcb-f89e98006b37";
#else

            if (string.IsNullOrEmpty(LibRedminePower.Properties.Settings.Default.AnonymousClientId))
            {
                LibRedminePower.Properties.Settings.Default.AnonymousClientId = createClientId();
                LibRedminePower.Properties.Settings.Default.Save();
            }
            return LibRedminePower.Properties.Settings.Default.AnonymousClientId;
#endif
        }

        private string createClientId()
        {
            try
            {
                var scope = new ManagementScope(@"\\" + Environment.MachineName + @"\root\cimv2");
                scope.Connect();
                using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("select SerialNumber from Win32_BIOS")))
                {
                    using (var mngObjs = searcher.Get())
                    {
                        var serialNo = string.Join("-", mngObjs.Cast<ManagementObject>().Select(o => o.GetPropertyValue("SerialNumber").ToString()).ToArray());
                        return $"{serialNo}_{Environment.MachineName}_{Environment.UserName}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to obtain serialNo. A random identifier was created instead. {ex}");
                return Guid.NewGuid().ToString();
            }
        }

        private string getUserAgent()
        {
            var uaArchitecture = (System.Environment.Is64BitProcess) ? "Win64; X64" : "Win32; X86";
            var os = System.Environment.OSVersion;
            var systemVersion = $"{os.Version.Major}.{os.Version.Minor}";
            return $"Mozilla/5.0 (Windows NT {systemVersion}; {uaArchitecture}; {ApplicationInfo.CompanyName}; {ApplicationInfo.Title})";
        }

        public void TrackAtomicFeature(string feature)
        {
            // The value of the "feature" string consists of the whole name of the tracked feature, 
            // for example : "MyGridView.Sorted.Name.Ascending", if we have performed a sorting operation in RadGridView. 
            // So, we can split this string in order to pass friendlier names to the parameters of the CreateCustomEvent method which will be used in your reports. 
            string category;
            string eventAction;
            this.splitFeatureName(feature, out category, out eventAction);

            var data = HitBuilder.CreateCustomEvent(category, eventAction + " event", feature.ToString(), 1).Build();
            tracker.Send(data);

            var dataGA4 = HitBuilder.CreateCustomEventGA4(feature.ToString()).Build();
            trackerManager.IsGA4 = true;
            trackerGA4.SendGA4(dataGA4);
            trackerManager.IsGA4 = false;
        }

        public void TrackError(string feature, Exception exception)
        {
            var text = feature + ":" + exception.ToString();
            // 送信データ量に制限があるため、文字数を制限する
            var textLimit = 5000;
            if (text.Length > textLimit)
                text = text.Substring(0, textLimit);
            var data = HitBuilder.CreateException(text, true).Build();
            tracker.Send(data);
        }

        public void TrackFeatureCancel(string feature)
        {
            string category;
            string eventAction;
            this.splitFeatureName(feature, out category, out eventAction);

            var data = HitBuilder.CreateCustomEvent(category, eventAction + " event.Cancelled", feature.ToString(), 1).Build();
            tracker.Send(data);
        }

        public void TrackFeatureStart(string feature)
        {
            string category;
            string eventAction;
            this.splitFeatureName(feature, out category, out eventAction);

            // Measuring timings provides a native way to measure a period of time in Google Analytics.  
            // This can be useful to measure resource load times, for example. 
            TimeSpan ts = TimeSpan.FromSeconds(2.2);
            var loadTiming = HitBuilder.CreateTiming(category, eventAction, ts).Build();
            tracker.Send(loadTiming);
        }

        public void TrackFeatureEnd(string feature)
        {
            string category;
            string eventAction;
            this.splitFeatureName(feature, out category, out eventAction);

            TimeSpan ts = TimeSpan.FromSeconds(2.2);
            var unLoadTiming = HitBuilder.CreateTiming(category, eventAction, ts).Build();
            tracker.Send(unLoadTiming);
        }

        public void TrackValue(string feature, long value)
        {
            string category;
            string eventAction;
            this.splitFeatureName(feature, out category, out eventAction);

            var data = HitBuilder.CreateCustomEvent(category, eventAction + " event", feature.ToString(), value).Build();
            tracker.Send(data);
        }

        private void splitFeatureName(string feature, out string category, out string eventAction)
        {
            string[] parameters = feature.Split('.');
            category = parameters[0];
            eventAction = parameters[1];
        }
    }
}