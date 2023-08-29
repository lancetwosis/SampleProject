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
    public enum TermInputValidationType
    {
        [LocalizedDescription(nameof(Resources.enumTermInputTypeNone), typeof(Resources))]
        None,
        [LocalizedDescription(nameof(Resources.enumTermInputTypeRequiredInput), typeof(Resources))]
        RequiredInput,
        [LocalizedDescription(nameof(Resources.enumTermInputTypeInputWarning), typeof(Resources))]
        InputWarning,
        [LocalizedDescription(nameof(Resources.enumTermInputTypeProhibitedInput), typeof(Resources))]
        ProhibitedInput,
        [LocalizedDescription(nameof(Resources.enumTermInputTypeNotInputWarning), typeof(Resources))]
        NotInputWarning,
    }

    public static class TermInputValidationTypeEx
    {
        public static string GetMessage(this TermInputValidationType target, TimeSpan start, TimeSpan end)
        {
            switch (target)
            {
                case TermInputValidationType.RequiredInput:
                    return string.Format(Resources.TermInputValidationMsgRequired, $"{start.ToString(@"hh\:mm")} - {end.ToString(@"hh\:mm")}");
                case TermInputValidationType.InputWarning:
                    return string.Format(Resources.TermInputValidationMsgNotRecommended, $"{start.ToString(@"hh\:mm")} - {end.ToString(@"hh\:mm")}");
                case TermInputValidationType.ProhibitedInput:
                    return string.Format(Resources.TermInputValidationMsgProhibuted, $"{start.ToString(@"hh\:mm")} - {end.ToString(@"hh\:mm")}");
                case TermInputValidationType.NotInputWarning:
                    return string.Format(Resources.TermInputValidationMsgRecommended, $"{start.ToString(@"hh\:mm")} - {end.ToString(@"hh\:mm")}");
                case TermInputValidationType.None:
                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
