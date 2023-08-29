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
    public enum TickLengthType
    {
        [LocalizedDescription(nameof(Resources.enumTickLength5), typeof(Resources))]
        TickLength5 = 5,
        [LocalizedDescription(nameof(Resources.enumTickLength10), typeof(Resources))]
        TickLength10 = 10,
        [LocalizedDescription(nameof(Resources.enumTickLength15), typeof(Resources))]
        TickLength15 = 15,
        [LocalizedDescription(nameof(Resources.enumTickLength30), typeof(Resources))]
        TickLength30 = 30,
    }

    public static class TickLengthTypeEx
    {
        public static FixedTickProvider ToTickProvider(this TickLengthType type)
        {
            switch (type)
            {
                case TickLengthType.TickLength5:
                    return new FixedTickProvider(new DateTimeInterval(5, 0, 0, 0, 0));
                case TickLengthType.TickLength10:
                    return new FixedTickProvider(new DateTimeInterval(10, 0, 0, 0, 0));
                case TickLengthType.TickLength15:
                    return new FixedTickProvider(new DateTimeInterval(15, 0, 0, 0, 0));
                case TickLengthType.TickLength30:
                    return new FixedTickProvider(new DateTimeInterval(30, 0, 0, 0, 0));
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 最小の表示幅（分）を基準に一日の表示領域（pixel）を決定する。表示幅が小さくなるほど細かく刻む必要があるため全体の表示領域は大きくなる。
        /// https://docs.telerik.com/devtools/wpf/controls/radscheduleview/end-user-capabilities/timerulerconfiguration
        /// </summary>
        public static double GetDefaultMinTimeRulerExtent(this TickLengthType type)
        {
            switch (type)
            {
                case TickLengthType.TickLength5:
                    return 6000;
                case TickLengthType.TickLength10:
                    return 3600;
                case TickLengthType.TickLength15:
                    return 2400;
                case TickLengthType.TickLength30:
                    return 1200;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// TickLengthType で設定された分数だけ加える。半端が出る場合、直近の割り切れる値を返す。
        /// 例えば、21:37 に 15 分加算しようとした場合、 21:45 が戻り値となる。
        /// </summary>
        public static DateTime AddMinutes(this DateTime dateTime, TickLengthType tickLength)
        {
            if (dateTime.Minute % (int)tickLength == 0)
            {
                return dateTime.AddMinutes((int)tickLength);
            }
            else
            {
                while (true)
                {
                    dateTime = dateTime.AddMinutes(1);
                    if (dateTime.Minute % (int)tickLength == 0)
                    {
                        return dateTime;
                    }
                }
            }
        }

        /// <summary>
        /// スライダーの％に応じて MinorTick の Background を Transparent にする必要があるかどうかを返す
        /// </summary>
        public static bool NeedsTransparent(this TickLengthType type, double sliderValue)
        {
            if (sliderValue == default(double))
                return false;

            switch (type)
            {
                case TickLengthType.TickLength5:
                    return sliderValue <= 70;
                case TickLengthType.TickLength10:
                case TickLengthType.TickLength15:
                case TickLengthType.TickLength30:
                    return sliderValue <= 60;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// スライダーの％に応じて MinorTick の Visibility を Collapsed にする必要があるかどうかを返す
        /// </summary>
        public static bool NeedsCollapsed(this TickLengthType type, double sliderValue)
        {
            if (sliderValue == default(double))
                return false;

            switch (type)
            {
                case TickLengthType.TickLength5:
                    return sliderValue <= 60;
                case TickLengthType.TickLength10:
                case TickLengthType.TickLength15:
                case TickLengthType.TickLength30:
                    return sliderValue <= 50;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
