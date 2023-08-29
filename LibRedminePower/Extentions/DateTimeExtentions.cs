using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class DateTimeExtentions
    {
        public static DateTime SetDay(this DateTime time, DateTime day)
        {
            return new DateTime(day.Year, day.Month, day.Day, time.Hour, time.Minute, time.Second);
        }

        /// <summary>
        /// 本時刻から end の日付まで一日ごと日付をインクリメントした DateTime のリストを返す
        /// </summary>
        public static IEnumerable<DateTime> GetDays(this DateTime start, DateTime end)
        {
            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                yield return date;
            }
        }

        /// <summary>
        /// 時刻部分を取り除き、年月日の部分のみを返す
        /// </summary>
        public static DateTime GetDateOnly(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        /// 本時刻が含まれる週の日付をリストで返す
        /// </summary>
        public static List<DateTime> GetThisWeekDays(this DateTime today)
        {
            List<DateTime> days = new List<DateTime>();
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            DateTime sunday = today.AddDays(daysUntilSunday);

            for (int i = 0; i < 7; i++)
            {
                days.Add(sunday.AddDays(i));
            }

            return days;
        }

        /// <summary>
        /// 本時刻が含まれる月の日付をリストで返す
        /// </summary>
        public static List<DateTime> GetThisMonthDays(this DateTime today)
        {
            List<DateTime> days = new List<DateTime>();
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            for (int i = 0; i < DateTime.DaysInMonth(today.Year, today.Month); i++)
            {
                days.Add(firstDayOfMonth.AddDays(i));
            }

            return days;
        }
    }
}
