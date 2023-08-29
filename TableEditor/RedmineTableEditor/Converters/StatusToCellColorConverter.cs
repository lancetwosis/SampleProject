using RedmineTicketPlanner.Models.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LibRedminePower.Extentions;
using System.Windows.Data;
using System.Windows;

namespace RedmineTicketPlanner.Converters
{

    public class StatusToCellColorConverter : IMultiValueConverter
    {
        private static SolidColorBrush ColorsTransparent = new SolidColorBrush(Colors.Transparent);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(x => x == DependencyProperty.UnsetValue)) 
                return ColorsTransparent;

            var statusId = (int?)values[0];
            if (statusId == null) return ColorsTransparent;
            var statusColors = (StatusColors)values[1];
            if (statusColors == null) return ColorsTransparent;
            if (!statusColors.IsEnabled) return ColorsTransparent;
            var colorModel = statusColors.Items.SingleOrDefault(a => a.Id == statusId);
            if (colorModel == null) return ColorsTransparent;
            return colorModel.Color.ToBrush();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
