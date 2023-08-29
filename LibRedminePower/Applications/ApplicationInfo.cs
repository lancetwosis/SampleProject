using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;

namespace LibRedminePower.Applications
{
    public static class ApplicationInfo
    {
        private static Assembly assembly = Assembly.GetEntryAssembly();

        public static string Title => getCustomAttribute<AssemblyTitleAttribute>().Title;
        public static Version Version => getVersion();
        public static string CopyRight => getCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public static string CompanyName => getCustomAttribute<AssemblyCompanyAttribute>().Company;
        public static string LogFolderName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName, Title);

        static private T getCustomAttribute<T>() where T : Attribute => (T)Attribute.GetCustomAttribute(assembly, typeof(T));

        private static Version getVersion()
        {
            // アセンブリ情報の４ケタ目を空欄にしても「0」が設定されてしまう
            // そのために「6.3.0.0」のような場合「6.3.0」になるように修正する
            var v = assembly.GetName().Version;
            if (v.Revision == 0 || v.MinorRevision == 0)
            {
                return new Version(v.Major, v.Minor, v.Build);
            }
            else
            {
                return v;
            }
        }
    }
}
