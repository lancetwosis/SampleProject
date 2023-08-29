using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Interfaces
{
    // Telerik にも同様のクラスがあったが Start や End の set ができなかったため自作する
    public interface IPeriod
    {
        DateTime Start { get; set; }
        DateTime End { get; set; }
    }

    public static class IPeriodEx
    {
        public static List<MyAppointment> Distinct(this List<MyAppointment> appointments)
        {
            return appointments.distinct((s, e) => new MyAppointment(s, e)).ToList();
        }

        public static List<MyTimeEntry> Distinct(this List<MyTimeEntry> entries)
        {
            return entries.distinct((s, e) => new MyTimeEntry(s, e)).ToList();
        }

        /// <summary>
        /// 重なった予定を一つの予定にまとめたリストを返す。
        /// 戻り値のリストの MyAppointment は RP などの初期化は行っていないので注意。
        /// </summary>
        private static IEnumerable<T> distinct<T>(this IEnumerable<T> periods, Func<DateTime, DateTime, T> creator) where T : IPeriod
        {
            var results = new List<T>();
            foreach (var a in periods.OrderBy(a => a.Start))
            {
                if (!results.Any())
                {
                    results.Add(creator.Invoke(a.Start, a.End));
                }
                else
                {
                    var last = results.Last();
                    if (last.End < a.Start)
                    {
                        results.Add(creator.Invoke(a.Start, a.End));
                    }
                    else
                    {
                        if (last.End < a.End)
                        {
                            last.End = a.End;
                        }
                    }
                }
            }
            return results;
        }


        public static List<MyAppointment> Subtract(this List<MyAppointment> source, List<MyAppointment> target)
        {
            return source.subtract(target, (s, e) => new MyAppointment(s, e));
        }

        public static List<MyTimeEntry> Subtract(this List<MyTimeEntry> source, List<MyTimeEntry> target)
        {
            return source.subtract(target, (s, e) => new MyTimeEntry(s, e));
        }

        /// <summary>
        /// 被っている部分を取り除いたリストを返す。どちらのリストも重なりがなく Start でソートされていること。
        /// </summary>
        private static List<T> subtract<T>(this List<T> source, List<T> target, Func<DateTime, DateTime, T> creator) where T : IPeriod
        {
            var result = new List<T>();
            foreach (var src in source)
            {
                var s = src;
                foreach (var tgt in target)
                {
                    var r = s.Subtract(tgt, creator);
                    if (!r.Any())
                    {
                        s = default(T);
                        break;
                    }
                    else if (r.Count == 1)
                    {
                        s = r[0];
                    }
                    else if (r.Count == 2)
                    {
                        result.Add(r[0]);
                        s = r[1];
                    }
                }
                if (s != null)
                    result.Add(s);
            }

            return result;
        }

        public static List<T> Subtract<T>(this T source, T target, Func<DateTime, DateTime, T> creator) where T : IPeriod
        {
            var result = new List<T>();

            if (target.Start <= source.Start &&
                source.Start < target.End && target.End < source.End)
            {
                result.Add(creator.Invoke(target.End, source.End));
            }
            else if (source.Start < target.Start && target.Start < source.End &&
                     source.End <= target.End)
            {
                result.Add(creator.Invoke(source.Start, target.Start));
            }
            else if (source.Start < target.Start && target.End < source.End)
            {
                result.Add(creator.Invoke(source.Start, target.Start));
                result.Add(creator.Invoke(target.End, source.End));
            }
            else if (target.Start <= source.Start && source.End <= target.End)
            {
                // 何もしない
            }
            else
            {
                result.Add(source);
            }

            return result;
        }

        /// <summary>
        /// start と end の間のみを取り出したリストを返す。どちらのリストも重なりがなく Start でソートされていること。
        /// </summary>
        public static List<MyAppointment> Take(this List<MyAppointment> appointments, DateTime start, DateTime end)
        {
            var result = new List<MyAppointment>();
            foreach (var a in appointments)
            {
                if (a.Start <= start && start < a.End && a.End < end)
                {
                    result.Add(new MyAppointment(start, a.End));
                }
                else if (start < a.Start && a.Start < end && end <= a.End)
                {
                    result.Add(new MyAppointment(a.Start, end));
                }
                else if (start < a.Start && a.End < end)
                {
                    result.Add(new MyAppointment(a.Start, a.End));
                }
            }
            return result;
        }
    }
}
