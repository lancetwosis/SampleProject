using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Logging
{
    public class Logger
    {
        private static NLog.Logger _logger;

        public static void Init()
        {
            NLog.LogManager.LoadConfiguration(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "NLog.config"));
            // Setup.iss の設定にて空白を含むソース名を取り扱えなかったため「_」で Replace する
            _logger = NLog.LogManager.GetLogger("ApplicationEventLog").WithProperty("SourceName", Applications.ApplicationInfo.Title.Replace(" ", "_"));
        }

        public static void Error(string message, params object[] args)
        {
            Error(null, message, args);
        }
        public static void Error(Exception exception, string message, params object[] args)
        {
            log(NLog.LogLevel.Error, exception, message, args);
        }
        public static void Warn(string message, params object[] args)
        {
            Warn(null, message, args);
        }
        public static void Warn(Exception exception, string message, params object[] args)
        {
            log(NLog.LogLevel.Warn, exception, message, args);
        }
        public static void Info(string message, params object[] args)
        {
            log(NLog.LogLevel.Info, null, message, args);
        }
        public static void Debug(string message, params object[] args)
        {
            log(NLog.LogLevel.Debug, null, message, args);
        }

        private static void log(NLog.LogLevel level, Exception exception, string message, params object[] args)
        {
            if (_logger == null) return;
            var logEvent = new NLog.LogEventInfo(level, _logger.Name, null, message, args, exception);
            _logger.Log(typeof(Logger), logEvent);

        }

        public static StopWatchLogger CreateProcess(string processName) => new StopWatchLogger(processName);
        public static StopWatchLogger CreateProcess<T>(string processName) => new StopWatchLogger<T>(processName);
    }
}
