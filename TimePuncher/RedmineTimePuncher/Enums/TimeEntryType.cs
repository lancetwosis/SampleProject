using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls.ColorPicker.Helpers;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TimeEntryType
    {
        [LocalizedDescription(nameof(Resources.enumTimeEntryTypeNotSpecified), typeof(Resources))]
        NotSpecified,
        [LocalizedDescription(nameof(Resources.enumTimeEntryTypeOnTime), typeof(Resources))]
        OnTime,
        [LocalizedDescription(nameof(Resources.enumTimeEntryTypeOverTime), typeof(Resources))]
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
                    return ColorEx.ToBrush("#cccccc");
                case TimeEntryType.OnTime:
                    // https://www.colordic.org/colorscheme/cce7ff
                    return ColorEx.ToBrush("#cce7ff");
                case TimeEntryType.OverTime:
                    // https://www.colordic.org/colorscheme/ffcce7
                    return ColorEx.ToBrush("#ffcce7");
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
