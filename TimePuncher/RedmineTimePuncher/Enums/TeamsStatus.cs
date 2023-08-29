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
    public enum TeamsStatus
    {
        [LocalizedDescription(nameof(Resources.TeamsStatusUndefind), typeof(Resources))]
        undefined,
        [LocalizedDescription(nameof(Resources.TeamsStatusUndefind), typeof(Resources))]
        Unknown,
        [LocalizedDescription(nameof(Resources.TeamsStatusAvailable), typeof(Resources))]
        Available,
        [LocalizedDescription(nameof(Resources.TeamsStatusOnThePhone), typeof(Resources))]
        OnThePhone,
        [LocalizedDescription(nameof(Resources.TeamsStatusBusy), typeof(Resources))]
        Busy,
        [LocalizedDescription(nameof(Resources.TeamsStatusAway), typeof(Resources))]
        Away,
        [LocalizedDescription(nameof(Resources.TeamsStatusBeFightBack), typeof(Resources))]
        BeFightBack,
        [LocalizedDescription(nameof(Resources.TeamsStatusDoNotDisturb), typeof(Resources))]
        DoNotDisturb,
        [LocalizedDescription(nameof(Resources.TeamsStatusPresenting), typeof(Resources))]
        Presenting,
        [LocalizedDescription(nameof(Resources.TeamsStatusInAMeeting), typeof(Resources))]
        InAMeeting,
        [LocalizedDescription(nameof(Resources.TeamsStatusNewActivity), typeof(Resources))]
        NewActivity,
        [LocalizedDescription(nameof(Resources.TeamsStatusOffline), typeof(Resources))]
        Offline,
        [LocalizedDescription(nameof(Resources.TeamsStatusConnectionError), typeof(Resources))]
        ConnectionError,
    }
}
