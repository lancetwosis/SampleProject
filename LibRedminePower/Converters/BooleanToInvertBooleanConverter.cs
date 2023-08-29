using System;
using System.Globalization;
using System.Windows.Data;

namespace LibRedminePower.Converters
{
    public class BooleanToInvertBooleanConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, bool>
    {
        public override bool Convert(bool value, object parameter, CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            return !val;
        }

        public override bool ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            return !val;
        }
    }
}
