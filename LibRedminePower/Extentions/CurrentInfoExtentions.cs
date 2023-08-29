using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class CurrentInfoExtentions
    {
        public static bool IsJp(this CultureInfo culture)
        {
            return culture.Name == "ja-JP";
        }

        public static Encoding GetEncoding(this CultureInfo culture)
        {
            return IsJp(culture) ? Encoding.GetEncoding("Shift_JIS") : Encoding.UTF8;
        }
    }
}
