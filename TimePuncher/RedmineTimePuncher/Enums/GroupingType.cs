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
using Telerik.Windows.Controls.TreeMap;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GroupingType
    {
        [LocalizedDescription(nameof(Resources.ShowEntryGroupUserName), typeof(Resources))]
        UserName,
        [LocalizedDescription(nameof(Resources.ShowEntryGroupIssueId), typeof(Resources))]
        IssueId,
        [LocalizedDescription(nameof(Resources.ShowEntryGroupActivity), typeof(Resources))]
        ActivityName,
    }

    public static class GroupingTypeEx
    {
        public static PivotMapGroupDefinition ToGroupDefinition(this GroupingType type)
        {
            switch (type)
            {
                case GroupingType.UserName:
                    return App.Current.Resources["UserNameGroupDefinition"] as PivotMapGroupDefinition;
                case GroupingType.IssueId:
                    return App.Current.Resources["IssueIdGroupDefinition"] as PivotMapGroupDefinition;
                case GroupingType.ActivityName:
                    return App.Current.Resources["ActivityNameGroupDefinition"] as PivotMapGroupDefinition;
                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
