using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace LibRedminePower.Converters
{
    [ContentProperty(nameof(Converters))]
    public class RelayValueConverterGroup : IValueConverter
    {
        public Collection<IValueConverter> Converters { get; } = new Collection<IValueConverter>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value;

            if (Converters == null) return result;

            foreach (var conv in Converters)
            {
                result = conv.Convert(result, targetType, parameter, culture);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value;

            if (Converters == null) return result;

            foreach (var conv in Converters.Reverse())
            {
                result = conv.ConvertBack(result, targetType, parameter, culture);
            }

            return result;
        }
    }
}
