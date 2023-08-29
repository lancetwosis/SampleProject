using RedmineTimePuncher.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Models.Managers
{
    public class DebugDataManager
    {
        private const string cFileName = "DebugData.csv";
        private string debugDataFile;

        /// <summary>
        /// 予定やメールに関してデバッグ用のデータを使うかどうかのフラグ。
        /// exe が配置されている場所に DebugData.csv があるかどうかで判定。
        /// データの作り方などはこちら http://133.242.159.37/issues/422
        /// </summary>
        public bool IsExist => System.IO.File.Exists(debugDataFile);

        public DebugDataManager()
        {
            var myAssembly = Assembly.GetEntryAssembly();
            var path = new System.IO.FileInfo(myAssembly.Location);
            debugDataFile = System.IO.Path.Combine(path.Directory.FullName, cFileName);
        }

        public List<MyAppointment> GetData(Resource resource, DateTime start, DateTime end, AppointmentType myType)
        {
            var lines = readAllLines(debugDataFile);
            return lines.Skip(1).Select(line => line.Split(','))
                .Select(a => (start: DateTime.Parse(a[0]), end: DateTime.Parse(a[1]), myType: a[2], title: a[3], ticketNo: a[4]))
                .Where(x => start <= x.start).Where(x => x.end <= end).Where(a => a.myType == myType.ToString())
                .Select(a => new MyAppointment(resource, (AppointmentType)Enum.Parse(typeof(AppointmentType), a.myType), a.title, "", a.start, a.end, a.ticketNo)).ToList();
        }

        public string[] readAllLines(string fileName)
        {
            var result = new List<string>();
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        result.Add(line);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
