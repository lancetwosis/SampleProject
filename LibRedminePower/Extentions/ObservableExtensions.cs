using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class ObservableExtensions
    {
        public static IDisposable SubscribeWithErr<T>(this IObservable<T> observable, Action<T> onNext)
        {
            return observable.Subscribe(x =>
            {
                try
                {
                    onNext(x);
                }
                catch (Exception ex)
                {
                    ErrorHandler.Instance.HandleError(ex);
                }
            });
        }

        public static IObservable<TResult> CombineLatestWithErr<TSource1, TSource2, TResult>(this IObservable<TSource1> first, IObservable<TSource2> second, Func<TSource1, TSource2, TResult> resultSelector)
        {
            return first.CombineLatest(second, (f, s) =>
            {
                try
                {
                    return resultSelector.Invoke(f, s);
                }
                catch (Exception ex)
                {
                    ErrorHandler.Instance.HandleError(ex);
                    return default(TResult);
                }
            });
        }

        public static IObservable<TResult> SelectManyIfNotNull<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, IObservable<TResult>> selector)
            where TSource : class
        {
            return source.SelectMany(item => item != null ? selector(item) : Observable.Return<TResult>(default));
        }
    }
}
