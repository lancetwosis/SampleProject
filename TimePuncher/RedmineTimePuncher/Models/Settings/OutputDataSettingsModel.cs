using LibRedminePower.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class OutputDataSettingsModel : Bases.SettingsModelBase<OutputDataSettingsModel>
    {
        public OutputCsvExportSettingsModel CsvExport { get; set; } = new OutputCsvExportSettingsModel();
        public OutputExtToolSettingsModel ExtTool { get; set; } = new OutputExtToolSettingsModel();

        public static string FILE_NAME = "{filename}";

        public Process ExportToExtTool(DateTime curDate, IEnumerable<MyAppointment> targets)
        {
            if (!System.IO.File.Exists(ExtTool.FileName))
            {
                // ExtTool.Error を起動するため設定しなおす
                var notExist = ExtTool.FileName;
                ExtTool.FileName = "";
                ExtTool.FileName = notExist;
                throw new ApplicationException(Properties.Resources.SettingsExpoMsgExternalToolNotExist);
            }

            // ファイルを出力する。
            var argFileName = System.IO.Path.GetTempFileName();
            CsvExport.Export(argFileName, targets);

            var argment = string.IsNullOrEmpty(ExtTool.Argument) ? FILE_NAME : ExtTool.Argument;
            var arg = Regex.Replace(argment, FILE_NAME, argFileName, RegexOptions.IgnoreCase);
            var p = Process.Start(ExtTool.FileName, arg);
            p.Exited += (s, e) =>
            {
                try
                {
                    System.IO.File.Delete(argFileName);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Argfile can not delete. {ex.ToString()}");
                }
            };
            return p;
        }
    }
}
