using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RedmineTimePuncher.Converters
{
    public class BoolToFontItalicConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, FontStyle>
    {
        public override FontStyle Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ? FontStyles.Italic : FontStyles.Normal; 
        }

        public override bool ConvertBack(FontStyle value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
