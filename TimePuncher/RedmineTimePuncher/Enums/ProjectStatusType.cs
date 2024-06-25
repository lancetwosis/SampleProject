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
    public enum ProjectStatusType
    {
        [Description("")]
        NotDefined,
        // 有効な場合、表示する必要がないので空文字とする
        [Description("")]
        Active,
        [LocalizedDescription(nameof(Resources.enumProjectStatusTypeNotActive), typeof(Resources))]
        NotActive,
    }
}
