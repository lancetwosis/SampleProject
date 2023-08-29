using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Concurrency;

namespace LibRedminePower.Extentions
{
    public static class ObservableCollectionExtentions
    {
        public static IObservable<T> StartWithDefault<T>(this IObservable<T> items) 
            => items.StartWith(new T[] { default });
        //public static IObservable<T> StartWithDefault<T>(this IFilteredReadOnlyObservableCollection<T> items) where T : class, INotifyPropertyChanged
        //    => items.StartWith(new T[] { default });

        public static IObservable<bool> AnyAsObservable<T>(this ObservableCollection<T> items) 
            => items.CollectionChangedAsObservable().StartWithDefault().Select(_ => items.Any());
        public static IObservable<bool> AnyAsObservable<T>(this IFilteredReadOnlyObservableCollection<T> items) where T : class, INotifyPropertyChanged
            => items.CollectionChangedAsObservable().StartWithDefault().Select(_ => items.Any());

        public static IObservable<bool> AnyAsObservable<T>(this ObservableCollection<T> items, Func<T, bool> predicate)
            => items.CollectionChangedAsObservable().StartWithDefault().Select(_ => items.Any(predicate));

        public static IObservable<bool> AnyAsObservable<T, TProperty>(this ObservableCollection<T> items, Func<T, bool> predicate, Expression<Func<T, IObservable<TProperty>>> propertySelector) where T : class, INotifyPropertyChanged
        {
            return items.CollectionChangedAsObservable().StartWithDefault()
                .CombineLatest(items.ObserveElementObservableProperty(propertySelector).StartWithDefault(),
                (_, __) => items.Any(predicate));
        }

        public static IObservable<bool> AnyAsObservable<T, TProperty>(this IFilteredReadOnlyObservableCollection<T> items, Func<T, bool> predicate, Expression<Func<T, TProperty>> propertySelector) where T : class, INotifyPropertyChanged
        {
            return items.CollectionChangedAsObservable().StartWithDefault()
                .CombineLatest(items.ObserveElementProperty(propertySelector).StartWithDefault(),
                (_, __) => items.Any(predicate));
        }

        public static IObservable<TProperty> MaxAsObservable<T, TProperty>(this ObservableCollection<T> items, Func<T, TProperty> predicate, Expression<Func<T, TProperty>> propertySelector) where T : class, INotifyPropertyChanged
        {
            return items.CollectionChangedAsObservable().StartWithDefault()
                .CombineLatest(items.ObserveElementProperty(propertySelector).StartWithDefault(),
                (_, __) => items.Any() ? items.Max(predicate) : default);
        }
    }
}
