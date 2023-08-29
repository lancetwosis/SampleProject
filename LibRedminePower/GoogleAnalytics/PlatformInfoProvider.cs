using GoogleAnalytics;
using System;

namespace LibRedminePower.GoogleAnalytics
{
    internal class PlatformInfoProvider : IPlatformInfoProvider
    {
        public string AnonymousClientId { get; set; }

        public int? ScreenColors { get; set; }

        public Dimensions ScreenResolution { get; set; }
        public string UserAgent { get; set; }
        public string UserLanguage { get; set; }
        public Dimensions ViewPortResolution { get; set; }

        Dimensions? IPlatformInfoProvider.ScreenResolution { get; }

        Dimensions? IPlatformInfoProvider.ViewPortResolution { get; }

        public event EventHandler ScreenResolutionChanged;
        public event EventHandler ViewPortResolutionChanged;

        public void OnTracking()
        {
            throw new NotImplementedException();
        }
    }
}
