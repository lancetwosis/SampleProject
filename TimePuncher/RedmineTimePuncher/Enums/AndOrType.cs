using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AndOrType
    {
        [LocalizedDescription(nameof(Resources.enumAndOrTypeAnd), typeof(Resources))]
        And,
        [LocalizedDescription(nameof(Resources.enumAndOrTypeOr), typeof(Resources))]
        Or,
    }
}
