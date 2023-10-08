using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize.Factors
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FactorValueType
    {
        [LocalizedDescription(nameof(Resources.enumFactorTypeNone), typeof(Resources))]
        None,
        [LocalizedDescription(nameof(Resources.enumFactorTypeDate), typeof(Resources))]
        Date,
        [LocalizedDescription(nameof(Resources.enumFactorTypeIssue), typeof(Resources))]
        Issue,
        [LocalizedDescription(nameof(Resources.enumFactorTypeProject), typeof(Resources))]
        Project,
        [LocalizedDescription(nameof(Resources.enumFactorTypeUser), typeof(Resources))]
        User,
        [LocalizedDescription(nameof(Resources.enumFactorTypeCategory), typeof(Resources))]
        Category,
        [LocalizedDescription(nameof(Resources.enumFactorTypeSortASC), typeof(Resources))]
        ASC,
        [LocalizedDescription(nameof(Resources.enumFactorTypeSortDESC), typeof(Resources))]
        DESC,
        [LocalizedDescription(nameof(Resources.enumFactorTypePosCenter), typeof(Resources))]
        Center,
        [LocalizedDescription(nameof(Resources.enumFactorTypePosTopLeft), typeof(Resources))]
        TopLeft,
        [LocalizedDescription(nameof(Resources.enumFactorTypePosTopRight), typeof(Resources))]
        TopRight,
        [LocalizedDescription(nameof(Resources.enumFactorTypePosBottomLeft), typeof(Resources))]
        BottomLeft,
        [LocalizedDescription(nameof(Resources.enumFactorTypePosBottomRight), typeof(Resources))]
        BottomRight,
        [LocalizedDescription(nameof(Resources.enumFactorTypeOnTime), typeof(Resources))]
        OnTime,
        // 画面に表示する箇所がないので不要
        IssueCustomField,
        [LocalizedDescription(nameof(Resources.enumFactorTypeFixedVersion), typeof(Resources))]
        FixedVersion,
    }
}
