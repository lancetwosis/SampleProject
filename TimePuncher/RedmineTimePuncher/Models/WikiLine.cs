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
            return matchHeader(type).Success;
        }

        public bool TryGetHeaderLevel(MarkupLangType type, out HeaderLevel level)
        {
            var m = matchHeader(type);
            if (m.Success)
                level = type.GetHeaderLevel(m.Groups[1].ToString());
            else
                level = HeaderLevel.None;

            return m.Success;
        }

        private Match matchHeader(MarkupLangType type)
        {
            return Regex.Match(Text, $"({type.GetHeaderPattern()})");
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
