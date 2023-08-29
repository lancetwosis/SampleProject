using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class StringExtentions
    {
        /// <summary>
        /// 文字列を行ごとに分解した配列を返す。\r\n, \n, \r のいずれにも対応。
        /// </summary>
        public static string[] SplitLines(this string str)
        {
            return str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        /// <summary>
        /// 文字列を指定された行数まで短くして返す。オーバーする場合、末尾に「...」を追加する。
        /// </summary>
        public static string LimitRows(this string str, int rowMax)
        {
            var lines = str.SplitLines().ToList();
            if (lines.Count() > rowMax)
            {
                lines = lines.Take(rowMax).ToList();
                lines.Add("...");
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
