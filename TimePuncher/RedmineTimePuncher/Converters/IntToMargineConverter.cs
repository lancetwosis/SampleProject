using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.Converters
{
    public class IntToMargineConverter : LibRedminePower.Converters.Bases.ConverterBase<int, Thickness>
    {
        public override Thickness Convert(int value, object parameter, CultureInfo culture)
        {
            var result = new Thickness();
            result.Left = value * 8;
            return result;
        }

        public override int ConvertBack(Thickness value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
