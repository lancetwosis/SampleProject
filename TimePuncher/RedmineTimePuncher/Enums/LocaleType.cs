using LibRedminePower.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LocaleType
    {
        Unselected,
        [Description("English")]
        EN,
        [Description("日本語")]
        JP,
    }

    public static class LocaleTypeEx
    {
        public static CultureInfo ToCultureInfo(this LocaleType localeType)
        {
            switch (localeType)
            {
                case LocaleType.JP:
                    return new CultureInfo("ja-JP");
                case LocaleType.EN:
                    return new CultureInfo("en-US");
                case LocaleType.Unselected:
                default:
                    throw new NotSupportedException($"localeType が {localeType} は、サポート対象外です。");
            }
        }

        public static LocaleType GetCurrent()
        {
            return CultureInfo.CurrentCulture.Name == "ja-JP" ? LocaleType.JP : LocaleType.EN;
        }

        public static LocaleType ToLocaleType(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return LocaleType.Unselected;

            switch (str)
            {
                case nameof(LocaleType.EN):
                    return LocaleType.EN;
                case nameof(LocaleType.JP):
                    return LocaleType.JP;
                default:
                    return LocaleType.Unselected;
            }
        }
    }
}
