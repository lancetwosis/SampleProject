using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize.Filters
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FilterType
    {
        [LocalizedDescription(nameof(Resources.VisualizeFilterEquals), typeof(Resources))]
        Equals,
        [LocalizedDescription(nameof(Resources.VisualizeFilterEqualsNot), typeof(Resources))]
        NotEquals
    }
}
