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
    public class BoolToFontBoldConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, FontWeight>
    {
        public override FontWeight Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ? FontWeights.Bold : FontWeights.Normal;
        }

        public override bool ConvertBack(FontWeight value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
