using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class CombineLatestEnumerableExtensions
    {
        public static IObservable<T> CombineLatestFirstOrDefault<T>(this IEnumerable<IObservable<T>> sources, Predicate<T> predicate)
        {
            return sources.CombineLatest(a => a.FirstOrDefault(b => predicate(b)));
        }
    }
}
