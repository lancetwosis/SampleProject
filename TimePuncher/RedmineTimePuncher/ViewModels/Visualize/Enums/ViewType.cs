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
using System.Windows.Media.Imaging;

namespace RedmineTimePuncher.ViewModels.Visualize.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ViewType
    {
        [LocalizedDescription(nameof(Resources.enumVisualizeViewTypeBar), typeof(Resources))]
        BarChart,
        [LocalizedDescription(nameof(Resources.enumVisualizeViewTypePie), typeof(Resources))]
        PieChart,
        [LocalizedDescription(nameof(Resources.enumVisualizeViewTypeTreeMap), typeof(Resources))]
        TreeMap
    }
}
