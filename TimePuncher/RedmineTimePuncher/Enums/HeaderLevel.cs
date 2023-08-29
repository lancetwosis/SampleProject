using LibRedminePower.Enums;
using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    public enum HeaderLevel
    {
        None = -1,
        H1 = 1,
        H2,
        H3,
        H4,
        H5,
        H6,
    }

    public static class HeaderLevelEx
    {
        public static string TEXTILE_PATTERN = @"h[1-6].";
        public static string MARKDOWN_PATTERN = @"#{1,6}";

        public static HeaderLevel FromTextileHeader(string str)
        {
            switch (str)
            {
                case "h1.": return HeaderLevel.H1;
                case "h2.": return HeaderLevel.H2;
                case "h3.": return HeaderLevel.H3;
                case "h4.": return HeaderLevel.H4;
                case "h5.": return HeaderLevel.H5;
                case "h6.": return HeaderLevel.H6;
                default:
                    throw new NotSupportedException();
            }
        }

        public static HeaderLevel FromMarkdownHeader(string str)
        {
            switch (str)
            {
                case "#":      return HeaderLevel.H1;
                case "##":     return HeaderLevel.H2;
                case "###":    return HeaderLevel.H3;
                case "####":   return HeaderLevel.H4;
                case "#####":  return HeaderLevel.H5;
                case "######": return HeaderLevel.H6;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
