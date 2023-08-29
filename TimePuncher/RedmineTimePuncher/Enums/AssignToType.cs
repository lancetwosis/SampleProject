using RedmineTimePuncher.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Attributes;
using Telerik.Windows.Controls.ScheduleView;
using LibRedminePower.Converters;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AssignToType
    {
        [LocalizedDescription(nameof(Resources.enumAssignToAny), typeof(Resources))]
        Any,
        [LocalizedDescription(nameof(Resources.enumAssignToMe), typeof(Resources))]
        Me,
        [LocalizedDescription(nameof(Resources.enumAssignToNotMe), typeof(Resources))]
        NotMe,
    }
}
