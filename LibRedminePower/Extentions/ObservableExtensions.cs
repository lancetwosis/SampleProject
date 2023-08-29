using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
