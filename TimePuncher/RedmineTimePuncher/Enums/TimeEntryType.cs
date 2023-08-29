using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.Enums
{
    public enum TimeEntryType
    {
        NotSpecified,
        OnTime,
        OverTime
    }

    public static class TimeEntryTypeEx
    {
        public static Brush GetColor(this TimeEntryType type)
        {
            switch (type)
            {
                case TimeEntryType.NotSpecified:
                    // https://www.colordic.org/colorsample/cccccc
                    return new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc));
                case TimeEntryType.OnTime:
                    // https://www.colordic.org/colorscheme/bffff4
                    return new SolidColorBrush(Color.FromRgb(0xbf, 0xff, 0xf4));
                case TimeEntryType.OverTime:
                    // https://www.colordic.org/colorscheme/ffc0cb
                    return new SolidColorBrush(Color.FromRgb(0xff, 0xc0, 0xcb));
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
