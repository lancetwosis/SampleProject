using LibRedminePower.Logging;
using LibRedminePower.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class SettingsExtentions
    {
        public static void SaveWithErr<T>(this T settings, bool onClosed = false) where T : ApplicationSettingsBase
        {
            try
            {
                settings.Save();
            }
            catch (Exception e)
            {
                var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                if (onClosed)
                    Logger.Error(e, $"Properties.Settings.Save() was failed to {conf.FilePath}");
                else
                    throw new ApplicationException(string.Format(Resources.msgErrFailedSaveSettings, conf.FilePath), e);
            }
        }
    }
}
