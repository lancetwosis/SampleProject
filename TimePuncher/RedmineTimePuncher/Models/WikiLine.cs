using LibRedminePower.Enums;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class WikiLine : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public int LineNo { get; set; }
        /// <summary>
        /// 一つのページ内での繰り返しの回数を表す
        /// </summary>
        public int RepeatCount { get; set; } = 1;
        public bool InCodeBlock { get; set; }
        public string Text { get; set; }

        public static WikiLine NOT_SPECIFIED = new WikiLine(-1, LibRedminePower.Properties.Resources.SettingsNotSpecified) { RepeatCount = 0 };

        public WikiLine() { }

        public WikiLine(int lineNo, string text)
        {
            LineNo = lineNo;
            Text = text;
        }

        public bool IsHeader(MarkupLangType type)
        {
            return InCodeBlock ? false : matchHeader(type).Success;
        }

        public HeaderLevel GetHeaderLevel(MarkupLangType type)
        {
            if (InCodeBlock)
                return HeaderLevel.None;

            var m = matchHeader(type);
            if (m.Success)
                return type.GetHeaderLevel(m.Groups[1].ToString());
            else
                return HeaderLevel.None;
        }

        private Match matchHeader(MarkupLangType type)
        {
            return Regex.Match(Text, $"({type.GetHeaderPattern()})");
        }

        public string GetImagePath(MarkupLangType type)
        {
            if (type == MarkupLangType.Textile)
            {
                // !{width:300px}clipboard-202412251448-difsc.png!
                var m1 = Regex.Match(Text, @"!(.+)!");
                if (m1.Success)
                {
                    var path = m1.Groups[1].ToString();
                    var m2 = Regex.Match(path, @"{.+}(.+)");
                    return m2.Success ? m2.Groups[1].ToString() : path;
                }

                return null;
            }
            else if (type == MarkupLangType.Markdown)
            {
                // ![テスト](clipboard-202412251448-difsc.png)
                var m1 = Regex.Match(Text, @"!\[.+\]\((.+)\)");
                if (m1.Success)
                    return m1.Groups[1].ToString();

                // Markdown でサイズを指定するには以下のように記述する必要がある
                // <img src="clipboard-202412251448-difsc.png" width="300px">
                var m2 = Regex.Match(Text, @"<img.* src=" + "\"(.+)\" ");
                if (m2.Success)
                    return m2.Groups[1].ToString();
            }

            return null;
        }

        public override string ToString()
        {
            if (Equals(NOT_SPECIFIED))
                return $"{Text}";
            else
                return $"{LineNo.ToString().PadLeft(4)} {Resources.SettingsReviLines} : {Text}";
        }

        /// <summary>
        /// 行数は挿入や削除で容易に変更されるため、行の中身と繰り返し回数のみで判定する。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is WikiLine line &&
                   RepeatCount == line.RepeatCount &&
                   Text == line.Text;
        }

        public override int GetHashCode()
        {
            int hashCode = -803697569;
            hashCode = hashCode * -1521134295 + RepeatCount.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            return hashCode;
        }
    }
}
