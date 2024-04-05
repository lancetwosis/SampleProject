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
        private string debugDataFile = "";

        private List<string> debugAppos = new List<string>()
        {
            // Start,End,タイプ,タイトル,本文,チケット番号
            //$"2024/3/12 10:00:00,2024/3/12 11:00:00,{AppointmentType.Schedule},予定のテスト,予定の本文,",
            //$"2024/3/12 15:00:00,2024/3/12 16:00:00,{AppointmentType.Mail},メールのテスト,メールの本文,",
            //$"2024/3/12 10:30:00,2024/3/12 11:05:00,{AppointmentType.TeamsCall},Oka Youichi からの着信,,",
            //$"2024/3/12 10:05:00,2024/3/12 10:20:00,{AppointmentType.RedmineActivity},#1263 (新規):v8.1.0 を新規インストール後の起動でアプリが落ちる,,1263",
            //$"2024/3/12 10:10:00,2024/3/12 10:25:00,{AppointmentType.RedmineActivity},#1263 (新規 → 進行中):v8.1.0 を新規インストール後の起動でアプリが落ちる,,1263",
        };

        /// <summary>
        /// 予定やメールに関してデバッグ用のデータを使うかどうかのフラグ。
        /// exe が配置されている場所に DebugData.csv があるかどうかで判定。（デバッグ実行時のみ debugAppos も参照）
        /// データの作り方などはこちら http://133.242.159.37/issues/422
        /// </summary>
        public bool IsExist 
        {
            get
            {
#if DEBUG
                return debugAppos.Any() || System.IO.File.Exists(debugDataFile);
#else
                return System.IO.File.Exists(debugDataFile);
#endif
            }
        }

        public DebugDataManager()
        {
            var myAssembly = Assembly.GetEntryAssembly();
            var path = new System.IO.FileInfo(myAssembly.Location);
            debugDataFile = System.IO.Path.Combine(path.Directory.FullName, cFileName);
        }

        public List<MyAppointment> GetData(Resource resource, DateTime start, DateTime end, AppointmentType myType)
        {
            var lines = readLines();
            return lines.Select(line => line.Split(','))
                .Select(a => (start: DateTime.Parse(a[0]), end: DateTime.Parse(a[1]), myType: a[2], title: a[3],body: a[4], ticketNo: a[5]))
                .Where(x => start <= x.start).Where(x => x.end <= end).Where(a => a.myType == myType.ToString())
                .Select(a => new MyAppointment(resource, (AppointmentType)Enum.Parse(typeof(AppointmentType), a.myType), a.title, a.body, a.start, a.end, a.ticketNo)).ToList();
        }

        public string[] readLines()
        {
            if (debugAppos.Any())
            {
                return debugAppos.ToArray();
            }
            else
            {
                var result = new List<string>();
                using (var fs = new FileStream(debugDataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                return result.Skip(1).ToArray();
            }
        }
    }
}
