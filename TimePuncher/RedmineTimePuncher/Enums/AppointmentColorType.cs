using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AppointmentColorType
    {
        [LocalizedDescription(nameof(Resources.enumFactorTypeCategory), typeof(Resources))]
        Category,
        [LocalizedDescription(nameof(Resources.enumFactorTypeProject), typeof(Resources))]
        Project,
    }
}
