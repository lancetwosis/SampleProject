using static SimpleExec.Command;
using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using RedmineTimePuncher.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Diagnostics;
using RedmineTimePuncher.ViewModels.Input.Slots;
using RedmineTimePuncher.Models.Settings;
using Cysharp.Diagnostics;

namespace RedmineTimePuncher.Models.Managers
{
    /// <summary>
    /// https://www.alexbilz.com/post/2021-09-09-forensic-artifacts-microsoft-teams/
    /// </summary>
    public class TeamsManager : LibRedminePower.Models.Bases.ModelBase
    {
        public static int TickLength;
        public bool IsInstalled { get; }

        // インストール確認
        private static string installedFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Microsoft\Teams");
        private string deadFileName = Path.Combine(installedFolder, @".dead");

        private string logFolderName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Microsoft\Teams");

        private string msTeamParserExePath;
        private AppointmentTeamsSettingsModel _setting;
        private DebugDataManager debugDataManager = new DebugDataManager();

        public TeamsManager(AppointmentTeamsSettingsModel setting)
        {
            _setting = setting;
            if (!setting.IsEnabled) return;

            IsInstalled = isInstalled();

            msTeamParserExePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Pint-34287\test.exe");
            if (!File.Exists(msTeamParserExePath))
                throw new ApplicationException(Properties.Resources.errEnviromentError);
        }

        private bool isInstalled()
        {
            if (System.IO.Directory.Exists(installedFolder))
            {
                if(System.IO.File.Exists(deadFileName))
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        private DateTime getLatestLastWriteTime(DirectoryInfo dir, string searchPattern) => dir.GetFiles(searchPattern).Max(a => a.LastWriteTime);
        private DateTime getOldestCreationTime(DirectoryInfo dir, string searchPattern) => dir.GetFiles(searchPattern).Min(a => a.CreationTime);

        public async Task<List<MyAppointment>> GetCallAsync(Resource resource, CancellationToken token, DateTime start, DateTime end)
        {
            if (!IsInstalled) return new List<MyAppointment>();
            if (!_setting.IsEnabledCallHistory) return new List<MyAppointment>();

            if (debugDataManager.IsExist) return debugDataManager.GetData(resource, start, end, Enums.AppointmentType.TeamsCall);

            var indexedDbPaths = new[]
                {
                    Path.Combine(logFolderName, @"IndexedDB"),
                    Path.Combine(logFolderName, @"Partitions\msa\IndexedDB")
                }
                .Where(a => Directory.Exists(a))
                .SelectMany(a => new DirectoryInfo(a).GetDirectories())
                .Where(a => a.GetFiles("*.ldb").Any())
                .Where(a => new DateSpan(getOldestCreationTime(a, "*.ldb"), getLatestLastWriteTime(a, "*.ldb")).IntersectsWith(new DateSpan(start, end)))
                .ToList();
            var tasks = indexedDbPaths.Where(a => a != null).Select(indexedDbPath =>
            {
                return Task.Run(() =>
                {
                    // ディレクトリを一時フォルダへコピーする。
                    var copyIndexedDbPath = Path.Combine(Path.GetTempPath(), indexedDbPath.Name);
                    indexedDbPath.CopyTo(copyIndexedDbPath, token);

                    // Teamsログ解析プロセス実行
                    var outputFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    try
                    {
                        var si = new ProcessStartInfo(msTeamParserExePath, $"{copyIndexedDbPath} {outputFileName}");
                        si.WorkingDirectory = System.IO.Path.GetDirectoryName(msTeamParserExePath);
                        si.CreateNoWindow = true;
                        si.RedirectStandardError = true;
                        si.RedirectStandardOutput = true;
                        si.UseShellExecute = false;
                        using (var proc = new Process())
                        using (var ctoken = new CancellationTokenSource())
                        {
                            proc.EnableRaisingEvents = true;
                            proc.StartInfo = si;
                            var sbOutput = new StringBuilder();
                            proc.OutputDataReceived += (s, e) => sbOutput.AppendLine(e.Data);
                            proc.ErrorDataReceived += (s, e) => sbOutput.AppendLine(e.Data);
                            proc.Exited += (s, ev) =>
                            {
                                if (proc.ExitCode != 1) throw new Exception(sbOutput.ToString());
                                ctoken.Cancel();
                            };
                            // プロセスの開始
                            proc.Start();
                            // 非同期出力読出し開始
                            proc.BeginErrorReadLine();
                            proc.BeginOutputReadLine();
                            // 終了まで待つ
                            ctoken.Token.WaitHandle.WaitOne();
                        }
                    }
                    finally
                    {
                        // 一時フォルダを削除する。
                        try
                        {
                            if (Directory.Exists(copyIndexedDbPath))
                                Directory.Delete(copyIndexedDbPath, true);
                        }
                        catch (Exception)
                        { }
                    }

                    // 結果ファイルを解析する。
                    var result = new List<MyAppointment>();
                    dynamic data = JArray.Parse(File.ReadAllText(outputFileName).Replace("\"call-log\":", "\"callLog\":"));
                    System.IO.File.Delete(outputFileName);

                    var contacts = new List<dynamic>();
                    var calls = new List<dynamic>();
                    var meetings = new List<dynamic>();
                    foreach (var i in data)
                    {
                        if (i.record_type == "contact")
                            contacts.Add(i);
                        else if (i.record_type == "call")
                            calls.Add(i);
                        else if (i.record_type == "meeting")
                            meetings.Add(i);
                    }

                    foreach (var call in calls)
                    {
                        var prop = call.properties;
                        var callLog = prop.callLog;
                        if (callLog.callState != "missed")
                        {
                            var isFromMe = (bool)(callLog.callDirection == "outgoing");
                            var startTime = ((DateTime)callLog.startTime).ToLocalTime();
                            var endTime = ((DateTime)callLog.endTime).ToLocalTime();
                            if (endTime - startTime <= TimeSpan.FromMinutes(TickLength))
                                endTime = startTime + TimeSpan.FromMinutes(TickLength);

                            var attenders = new List<dynamic>();

                            if (isFromMe && callLog.targetParticipant != null)
                                attenders.Add(contacts.FirstOrDefault(a => a.mri == callLog.targetParticipant.id));
                            else if (!isFromMe && callLog.originatorParticipant != null)
                                attenders.Add(contacts.Where(a => a.mri == callLog.originatorParticipant.id).FirstOrDefault());
                            if (callLog.participants != null)
                                foreach (var p in callLog.participants)
                                    attenders.Add(contacts.FirstOrDefault(a => a.mri == p));
                            var title = (string)(attenders.FirstOrDefault()?.displayName) + (isFromMe ? " への発信" : " からの着信");
                            result.Add(new MyAppointment(resource, AppointmentType.TeamsCall, title, "", startTime, endTime, null, null));
                        }
                    }
                    foreach (var meeting in meetings)
                    {
                        var prop = meeting.threadProperties;
                        var meetingLog = prop.meeting;
                        string title = meetingLog.subject;
                        var startTime = ((DateTime)meetingLog.startTime).ToLocalTime();
                        var endTime = ((DateTime)meetingLog.endTime).ToLocalTime();
                        var attenders = new List<dynamic>();
                        foreach (var p in meeting.members)
                            attenders.Add(contacts.FirstOrDefault(a => a.mri == p.id));

                        result.Add(new MyAppointment(resource, AppointmentType.TeamsMeeting, title, "", startTime, endTime, null, null));
                    }
                    return result;
                });
            });
            var myResult = await Task.WhenAll(tasks);
            return myResult.SelectMany(a => a).Where(a => (start <= a.Start && a.Start <= end) || (start <= a.End && a.End <= end) || (a.Start <= start && end <= a.End)).ToList();
        }

        public List<Slot> GetStatus(Resource resource, DateTime start, DateTime end)
        {
            var result = new List<TeamsStatusSlot>();

            if (!IsInstalled) return result.Cast<Slot>().ToList();
            if (!_setting.IsEnabledStatus) return result.Cast<Slot>().ToList();

            var files = new System.IO.DirectoryInfo(logFolderName).GetFiles("*logs*").OrderBy(a => a.LastWriteTime);
            var alllines = files.SelectMany(a => readFile(a.FullName));

            var orgStatus = Enums.TeamsStatus.Unknown;
            var curStatus = Enums.TeamsStatus.Unknown;
            var teamsStatuss = Enum.GetValues(typeof(TeamsStatus)).Cast<TeamsStatus>().Where(a => a != TeamsStatus.NewActivity).ToList();
            var dateTime = DateTime.MinValue;
            foreach(var line in alllines)
            {
                if (line.Length < 24) continue;
                if(DateTime.TryParse(line.Substring(0,24), out dateTime))
                {
                    var updateFlag = false;
                    if (line.Contains($"StatusIndicatorStateService: Added") || line.Contains("Setting the taskbar overlay icon - "))
                    {
                        foreach (var targetState in teamsStatuss)
                        {
                            if (line.Contains($"Setting the taskbar overlay icon - {targetState.GetDescription()}") ||
                                line.Contains($"StatusIndicatorStateService: Added {targetState}") ||
                                line.Contains($"StatusIndicatorStateService: Added NewActivity (current state: {targetState} -> NewActivity"))
                            {
                                curStatus = targetState;
                                updateFlag = true;
                                break;
                            }
                        }
                    }
                    else if (line.Contains("System has been suspended"))
                    {
                        orgStatus = curStatus;
                        curStatus = TeamsStatus.Offline;
                        updateFlag = true;
                    }
                    else if (line.Contains("System has been resumed"))
                    {
                        curStatus = orgStatus;
                        updateFlag = true;
                    }
                    else if(line.Contains("session-end fired"))
                    {
                        curStatus = TeamsStatus.Offline;
                        updateFlag = true;
                    }
                    if(updateFlag)
                    {
                        if (!result.Any())
                            result.Add(new TeamsStatusSlot(DateTime.MinValue, dateTime, new[] { resource }, TeamsStatus.Unknown));
                        if(result.Last().Status != curStatus)
                        {
                            result.Last().End = dateTime;
                            result.Add(new TeamsStatusSlot(result.Last().End, DateTime.MaxValue, new[] { resource }, curStatus));
                        }
                    }
                }
            }

            result.Last().End = dateTime;
            return result.Where(a => (start <= a.Start && a.Start <= end) || (start <= a.End && a.End <= end) || (a.Start <= start && end <= a.End)).Cast<Slot>().ToList();
        }

        private List<string> readFile(string fileName)
        {
            var result = new List<string>();
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(fs))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        result.Add(line);
                    }
                }
            }
            return result;
        }
    }
}
