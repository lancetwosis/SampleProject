using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class EnumerableExtentions
    {
        public static IEnumerable<(T v, int i, bool isFirst, bool isLast)> Indexed<T>(this IEnumerable<T> source)
            => source.Select((v, i) => (v, i, isFirst: i == 0, isLast: i == source.Count() - 1));

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> values, int size)
        {
            if (size <= 0) throw new ArgumentException();

            while(values.Any())
            {
                yield return values.Take(size);
                values = values.Skip(size);
            }
        }

        public static IEnumerable<IEnumerable<T>> Pairs<T>(this IEnumerable<T> values)
        {
            var list = values.ToList();
            return list.Where((e, i) => i < list.Count - 1)
                .Select((e, i) => new[] { e, list[i + 1] });
        }

        public static StringCollection ToStringCollection(this IEnumerable<string> strings)
        {
            var stringCollection = new StringCollection();
            foreach (string s in strings) stringCollection.Add(s);
            return stringCollection;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> selector)
        {
            return getChildren(list, selector);
        }

        private static IEnumerable<T> getChildren<T>(IEnumerable<T> parents, Func<T, IEnumerable<T>> selector)
        {
            var result = new List<T>();
            foreach (var p in parents)
            {
                result.Add(p);
                var children = selector.Invoke(p);
                if (children != null && children.Any())
                   result.AddRange(getChildren(children, selector));
            }
            return result;
        }

        /// <summary>
        /// 指定された条件の要素が見つからなかった場合、第二引数の要素を返す
        /// </summary>
        public static T FirstOrDefault<T>(this IEnumerable<T> list, Func<T, bool> predicate, T defaultValue )
        {
            var result = list.FirstOrDefault(predicate);
            return result != null ? result : defaultValue;
        }

        /// <summary>
        /// comparer に従って同値判定を行い、Distinct したリストを返す。条件を満たすものが複数あった場合、最初の要素が追加される。
        /// </summary>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, T, bool> comparer)
        {
            var result = new List<T>();
            foreach (var i in list)
            {
                if (!result.Any(i2 => comparer(i, i2)))
                {
                    result.Add(i);
                }
            }
            return result;
        }
    }
}
