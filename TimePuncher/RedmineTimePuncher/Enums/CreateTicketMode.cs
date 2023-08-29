using LibRedminePower.Attributes;
using LibRedminePower.Converters;
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

    public enum CreateTicketMode
    {
        [LocalizedDescription(nameof(Resources.AppModeTicketCreater), typeof(Resources))]
        Review,
        [LocalizedDescription(nameof(Resources.AppModeTicketCreaterRequestWork), typeof(Resources))]
        Work
    }
}
