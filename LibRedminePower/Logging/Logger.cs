using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LibRedminePower.Logging
{
    public class Logger
    {
        public static string DECRYPT_KEY = "--decrypt-log-file";

        private static NLog.Logger _logger;
        private static string pId { get; set; }

        public static void Init()
        {
            var config = new LoggingConfiguration();
            var logDir = "${specialfolder:folder=ApplicationData}/Redmine Power/Redmine Studio/";
            var logFile = new FileTarget("logFile")
            {
                Encoding = Encoding.UTF8,
                Layout = "${message}",
                FileName = logDir + "RedmineStudio.log",
                ArchiveFileName = logDir + "RedmineStudio.{#}.zip",
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                EnableArchiveFileCompression = true,
                ArchiveAboveSize = 5000000, // 5 MB
                MaxArchiveFiles = 10,
            };
            var logConsole = new ConsoleTarget("logConsole") { Layout = "${message}" };

            config.AddRuleForAllLevels(logFile);
            config.AddRuleForAllLevels(logConsole);

            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();

            pId = Process.GetCurrentProcess().Id.ToString().PadLeft(5);
        }

        public static void Error(string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Error(null, message, callerMemberName, callerFilePath, callerLineNumber);
        }
        public static void Error(Exception exception, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            log(LogLevel.Error, exception, $"{message} (v{Applications.ApplicationInfo.Version})", callerMemberName, callerFilePath, callerLineNumber);
        }
        public static void Warn(string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Warn(null, message, callerMemberName, callerFilePath, callerLineNumber);
        }
        public static void Warn(Exception exception, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            log(LogLevel.Warn, exception, $"{message} (v{Applications.ApplicationInfo.Version})", callerMemberName, callerFilePath, callerLineNumber);
        }
        public static void Info(string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            log(LogLevel.Info, null, message, callerMemberName, callerFilePath, callerLineNumber);
        }
        public static void Debug(string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            log(LogLevel.Debug, null, message, callerMemberName, callerFilePath, callerLineNumber);
        }

        private static void log(LogLevel level, Exception exception, string message,
            string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            if (_logger == null) return;

            var tId = Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(2);
            var header = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} [{pId}][{tId}][{level.ToString().PadLeft(5)}]";
            var footer = $"{Path.GetFileNameWithoutExtension(callerFilePath)}.{callerMemberName}() : {callerLineNumber} (v{Applications.ApplicationInfo.Version})";
            var ex = exception != null ? $" {exception.GetType().Name}: {exception.Message}{Environment.NewLine}{exception.StackTrace}" : "";
            var msg =  $"{header} {message} - {footer}{ex}";
#if DEBUG
#else
            msg = msg.Encrypt();
#endif
            var logEvent = new LogEventInfo(level, _logger.Name, null, msg, null, exception);
            _logger.Log(typeof(Logger), logEvent);
        }

        public static StopWatchLogger CreateProcess(string processName) => new StopWatchLogger(processName);
        public static StopWatchLogger CreateProcess<T>(string processName) => new StopWatchLogger<T>(processName);

        public static void DecryptLogs()
        {
            var openLog = new OpenFileDialog();
            openLog.Title = "Please select the log file of Redmine Studio.";
            openLog.Filter = "Log Files|*.log" + "|All Files|*.*";
            var r = openLog.ShowDialog();
            if (!r.HasValue || !r.Value)
                return;

            var saveLog = new SaveFileDialog();
            saveLog.Filter = "Log Files|*.log" + "|All Files|*.*";
            saveLog.FileName = $"{Path.GetFileNameWithoutExtension(openLog.FileName)}_decrypted{Path.GetExtension(openLog.FileName)}";
            var r2 = saveLog.ShowDialog();
            if (!r2.HasValue || !r2.Value)
                return;

            try
            {
                var lines = File.ReadAllLines(openLog.FileName);
                var decrypted = lines.Select(l => l.DecryptOrDefault()).ToList();
                File.WriteAllLines(saveLog.FileName, decrypted);

                Process.Start(saveLog.FileName);
            }
            catch
            {
                MessageBoxHelper.ConfirmError($"Failed to decrypt the log file.{Environment.NewLine}{openLog.FileName}");
            }
        }
    }
}
