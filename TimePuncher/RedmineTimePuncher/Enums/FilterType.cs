using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    public enum FilterType
    {
        Contains,
        AsRegex,
    }

    public static class FilterTypeEx
    {
        public static bool IsMatch(this string title, FilterType type, string disableWords)
        {
            switch (type)
            {
                case FilterType.Contains:
                    return title.Contains(disableWords);
                case FilterType.AsRegex:
                    return Regex.IsMatch(title, disableWords);
                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
