using LibRedminePower.Applications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class OutputCsvExportSettingsModel : LibRedminePower.Models.Bases.ModelBase
    {
        public ObservableCollection<Enums.ExportItems> ExportItems { get; set; } = new ObservableCollection<Enums.ExportItems>(new[]
        {
            Enums.ExportItems.StartTime,
            Enums.ExportItems.EndTime,
            Enums.ExportItems.TicketFullName,
            Enums.ExportItems.WorkCategory,
            Enums.ExportItems.Subject,
        });
        public string ExportDir { get; set; }
        public bool IsOepn { get; set; } = true;
        public int ExportNum { get; set; } = 30;

        public void Export(DateTime curDate, IEnumerable<MyAppointment> targets)
        {
            // 出力先フォルダを取得する。
            var folderName = string.IsNullOrEmpty(ExportDir) ?
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ApplicationInfo.Title) :
                ExportDir;

            if (!System.IO.Directory.Exists(folderName))
                System.IO.Directory.CreateDirectory(folderName);

            // ファイルが規定数を超過している場合は削除する。
            var prefix = "RedmineTimePuncher_";
            if (ExportNum > 0)
            {
                var di = new System.IO.DirectoryInfo(folderName);
                var removeFiles = di.GetFiles().Where(a => a.Name.Contains(prefix)).OrderByDescending(a => a.Name).Skip(ExportNum - 1);
                if (removeFiles.Any())
                    removeFiles.ToList().ForEach(a =>
                    {
                        try
                        {
                            a.Delete();
                        }
                        catch (Exception) { }
                    });
            }

            // ファイル名を決定する。
            var fileName = System.IO.Path.Combine(
                folderName,
                prefix + curDate.ToString("yyyyMMdd") + ".csv");

            // ファイルを出力する。
            Export(fileName, targets);

            // 出力したファイルを開く
            if (IsOepn) 
                Process.Start(fileName);
        }

        public void Export(string fileName, IEnumerable<MyAppointment> targets)
        {
            System.IO.File.WriteAllLines(
                fileName,
                targets.OrderBy(a => a.Start).Select(a => a.ToCsvLine(ExportItems)),
                Encoding.GetEncoding("Shift_JIS"));
        }
    }
}
