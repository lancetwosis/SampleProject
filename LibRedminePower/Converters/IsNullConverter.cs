using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Converters
{
    public class IsNullConverter : Bases.ConverterBase<object, bool>
    {
        public override bool Convert(object value, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public override object ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
